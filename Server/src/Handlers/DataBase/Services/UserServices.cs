using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Server.Handlers.DataBase.Data;
using Server.Handlers.DataBase.Models;
using Server.Handlers.WS;

namespace Server.Handlers.DataBase.Services;

public class UserServices
{
    private readonly ApplicationDbContext _dbContext;
    
    public UserServices(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public Users CreateUser(Users user)
    {
        _dbContext.Users?.Add(user);
        _dbContext.SaveChanges();
        return user;
    }

    public IQueryable<Users>? GetUsers()
    {
        return _dbContext.Users;
    }

    public Users UpdateUser(Users user)
    {
        _dbContext.Users?.Update(user);
        _dbContext.SaveChanges();
        return user;
    }

    public void DeleteUser(int userId)
    {
        var user = _dbContext.Users?.Find(userId);
        if (user != null)
        {
            _dbContext.Users?.Remove(user);
            _dbContext.SaveChanges();
        }
    }
    
    public async Task<UserDtOs.RegisterResponse> Register(string? email, string? username, string? password)
    {
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email || u.Username == username);

        if (existingUser != null)
        {
            return new UserDtOs.RegisterResponse
            {
                UserId = 0,
                SessionId = Guid.Empty,
                SuccessMessage = "A user with this email or username already exists."
            };
        }

        var newUser = new Users
        {
            Email = email,
            Username = username,
            Password = HashPassword(password),
            SessionId = Guid.NewGuid()
        };

        await _dbContext.Users.AddAsync(newUser);
        await _dbContext.SaveChangesAsync();
        
        SocketServer.logger.LogInformation(3, "New Registration: E-Mail: {Email} & Username: {Username}", email, username);

        return new UserDtOs.RegisterResponse
        {
            UserId = newUser.Id,
            SessionId = newUser.SessionId,
            SuccessMessage = $"Registration successful. UserId: {newUser.Id}, SessionId: {newUser.SessionId}"
        };
    }

    public async Task<UserDtOs.LoginResponse> Login(string? usernameOrEmail, string? password, Socket socket)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == usernameOrEmail || u.Username == usernameOrEmail);

        if (user == null || !VerifyPassword(user.Password, password))
        {
            return new UserDtOs.LoginResponse
            {
                UserId = 0,
                SessionId = Guid.Empty,
                SuccessMessage = "Invalid username/email or password!"
            };
        }

        user.SessionId = Guid.NewGuid();
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
        
        return new UserDtOs.LoginResponse
        {
            UserId = user.Id,
            SessionId = user.SessionId,
            Username = user.Username,
            SuccessMessage = $"Login successful. UserId: {user.Id}, SessionId: {user.SessionId}"
        };
    }
    
    private string HashPassword(string? password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string? hashedPassword, string? passwordToCheck)
    {
        return BCrypt.Net.BCrypt.Verify(passwordToCheck, hashedPassword);
    }
}