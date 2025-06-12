using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smart_ERP.Data;
using Smart_ERP.DTOs;
using Smart_ERP.Modules.Auth.Models;
using Smart_ERP.Modules.Auth.Services;

namespace Smart_ERP.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly ERPDbContext _context;
        private readonly JwtService _jwtService;

        public AuthController(ERPDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            // Validate the input
            if (string.IsNullOrEmpty(dto.UserName) || string.IsNullOrEmpty(dto.Password))
            {
                return BadRequest("Username and password are required.");
            }

            // Check if the user already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u =>
                u.UserName == dto.UserName
            );
            if (existingUser != null)
            {
                return Conflict("Username already exists.");
            }
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == dto.Role);
            if (role == null)
                return BadRequest("Invalid role");

            // Create a new user
            var user = new User
            {
                UserName = dto.UserName,
                Email = dto.Email,
                Phone = dto.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
        {
            var user = await _context
                .Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserName == dto.UserName);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials");

            if (!user.IsActive)
                return Forbid("Account is deactivated");

            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user);
            return Ok(new { token });
        }

        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            // Check if the user exists
            var user = await _context
                .Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(user.ToString());
        }

        /*
        [HttpPut("user/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            // Check if the user exists
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Update user properties
            user.UserName = updateUserDto.UserName;
            user.Email = updateUserDto.Email;
            user.Phone = updateUserDto.Phone;
            user.IsActive = updateUserDto.IsActive;

            await _context.SaveChangesAsync();

            return Ok("User updated successfully.");
        }*/
        [HttpDelete("user/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            // Check if the user exists
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Delete the user
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok("User deleted successfully.");
        }

        // [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            // Get all users
            var users = await _context.Users.Include(u => u.Role).ToListAsync();
            return Ok(users.Select(u => u.ToString()));
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            // Get all roles
            var roles = await _context.Roles.ToListAsync();
            return Ok(roles);
        }

        /*
                #region fun based on roles
                [Authorize(Roles = "Admin, User")] // Allow both Admin and User roles to access
                [HttpGet("users")]
                public async Task<IActionResult> GetUsers()
                {
                    var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        
                    if (currentUserRole == "Admin")
                    {
                        // Get all users if the current user is an Admin
                        var allUsers = await _context.Users.Include(u => u.Role).ToListAsync();
                        return Ok(allUsers.Select(u => u.ToString()));
                    }
                    else if (currentUserRole == "User")
                    {
                        // Get only the current user's data if the current user is a User
                        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Assuming you are using Identity and have NameIdentifier claim
                        if (currentUserId != null)
                        {
                            var currentUser = await _context
                                .Users.Include(u => u.Role)
                                .Where(u => u.Id == currentUserId) // Assuming your User entity has an Id property
                                .FirstOrDefaultAsync();
        
                            if (currentUser != null)
                            {
                                return Ok(currentUser.ToString());
                            }
                            else
                            {
                                return NotFound("User not found."); // Handle the case where the user ID doesn't exist
                            }
                        }
                        else
                        {
                            return Unauthorized("User ID not found in claims."); // Handle the case where User ID claim is missing
                        }
                    }
                    else
                    {
                        return Unauthorized("Invalid role."); // Handle cases where the user has an unexpected role
                    }
                }
                #endregion
        */
        [HttpPost("role")]
        public async Task<IActionResult> CreateRole([FromBody] Role role)
        {
            // Validate the input
            if (string.IsNullOrEmpty(role.Name))
            {
                return BadRequest("Role name is required.");
            }

            // Check if the role already exists
            var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == role.Name);
            if (existingRole != null)
            {
                return Conflict("Role already exists.");
            }

            // Create a new role
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return Ok("Role created successfully.");
        }

        [HttpPut("role/{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] Role role)
        {
            // Check if the role exists
            var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);
            if (existingRole == null)
            {
                return NotFound("Role not found.");
            }

            // Update role properties
            existingRole.Name = role.Name;
            existingRole.Description = role.Description;

            await _context.SaveChangesAsync();

            return Ok("Role updated successfully.");
        }

        [HttpDelete("role/{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            // Check if the role exists
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);
            if (role == null)
            {
                return NotFound("Role not found.");
            }

            // Delete the role
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return Ok("Role deleted successfully.");
        }

        [HttpGet("role/{id}")]
        public async Task<IActionResult> GetRole(int id)
        {
            // Check if the role exists
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);
            if (role == null)
            {
                return NotFound("Role not found.");
            }

            return Ok(role);
        }

        [HttpGet("user/{id}/roles")]
        public async Task<IActionResult> GetUserRoles(int id)
        {
            // Check if the user exists
            var user = await _context
                .Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(user.Role);
        }

        [HttpPost("user/{id}/roles")]
        public async Task<IActionResult> AssignRoleToUser(int id, [FromBody] Role role)
        {
            // Check if the user exists
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Check if the role exists
            var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.Id == role.Id);
            if (existingRole == null)
            {
                return NotFound("Role not found.");
            }

            // Assign the role to the user
            user.Role = existingRole;
            await _context.SaveChangesAsync();

            return Ok("Role assigned to user successfully.");
        }
    }
}
