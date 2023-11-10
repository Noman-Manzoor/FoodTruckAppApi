using FoodTruckAppApi.Domain;
using FoodTruckAppApi.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FoodTruckAppApi.Services
{
    public class AccountService
    {
        private readonly FoodTruckAppContext _appContext;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AccountService(FoodTruckAppContext appContext, IPasswordHasher<User> passwordHasher)
        {
            _appContext = appContext;
            _passwordHasher = passwordHasher;
        }

        public string HashPassword(User user, string password)
        {
            // Hash the password
            string hashedPassword = _passwordHasher.HashPassword(user, password);
            return hashedPassword;
        }

        public async Task<OperationResult> CreateUser(UserDto userDto)
        {
            // Check if a user with the given username or email already exists.
            var existingUser = await _appContext.Users
                .AnyAsync(x => x.Email.ToLower().Trim() == userDto.Email.ToLower().Trim()
                            || x.UserName.Trim() == userDto.UserName.Trim());
            if (existingUser)
            {
                return new OperationResult(false, "User with the same email or username already exists.");
            }

            if (userDto.Id == 0) // Create user case
            {
                var newUser = new User 
                {
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    Email = userDto.Email,
                    UserName = userDto.UserName,
                    UserType = userDto.UserType,
                    ContactNo = userDto.ContactNo,
                    Medium = userDto.Medium,
                     Location= userDto.Location,
                    CreatedDate = DateTime.UtcNow
                };

                // Hash the password and set the PasswordHash field (assuming you have a method for this)
                newUser.Password = HashPassword(newUser, userDto.Password);

                // Persist the new user to the database.
                await _appContext.Users.AddAsync(newUser);
                await _appContext.SaveChangesAsync();

                return new OperationResult(true, "User created successfully.");
            }
            else // Update user case
            {
                var userToBeUpdated = await _appContext.Users
                   .FirstOrDefaultAsync(x => x.Id == userDto.Id);

                if (userToBeUpdated == null)
                {
                    return new OperationResult(false, "User not found.");
                }

                // Update only modifiable fields
                userToBeUpdated.FirstName = userDto.FirstName;
                userToBeUpdated.LastName = userDto.LastName;
                userToBeUpdated.Email = userDto.Email;
                userToBeUpdated.ContactNo = userDto.ContactNo;
                userToBeUpdated.Location = userDto.Location;

                // Save the changes
                _appContext.Users.Update(userToBeUpdated);
                await _appContext.SaveChangesAsync();

                return new OperationResult(true, "User updated successfully.");
            }
        }

        public async Task<OperationResult> AuthenticateUser(string emailOrUsername, string password)
        {
            // Normalize the email/username for consistent searching.
            string normalizedEmailOrUsername = emailOrUsername.ToLower().Trim();

            // Attempt to retrieve the user by username or email.
            var user = await _appContext.Users
                .FirstOrDefaultAsync(x => x.Email.ToLower().Trim() == emailOrUsername.ToLower().Trim() || x.UserName.Trim() == emailOrUsername.Trim());

            // If user not found, return an unsuccessful result.
            if (user == null)
            {
                return new OperationResult(false, "Invalid credentials.", null);
            }

            // Verify the password hash.
            PasswordVerificationResult passwordResult = _passwordHasher.VerifyHashedPassword(user, user.Password, password);

            if (passwordResult == PasswordVerificationResult.Failed)
            {
                return new OperationResult(false, "Invalid credentials.", null);
            }

            // If password is correct, generate a JWT token for the user.
            string token = GenerateJwtToken(user); // You'll need to implement this method.

            // Return a successful operation result with the token.
            return new OperationResult(true, "Authentication successful.", token);
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("your-256-bit-long-secret-key-goes-here!");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("FirstName", user.FirstName),
                    new Claim("LastName", user.LastName),
                    new Claim("Medium", user.Medium),
                    new Claim(ClaimTypes.Role, user.UserType)
                }),
                Expires = DateTime.UtcNow.AddHours(2), 
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
