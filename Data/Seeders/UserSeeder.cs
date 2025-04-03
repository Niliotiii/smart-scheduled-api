using SmartScheduledApi.Models;
using SmartScheduledApi.Enums;
using Microsoft.AspNetCore.Identity;

namespace SmartScheduledApi.Data.Seeders;

public static class UserSeeder
{
    public static readonly User[] Users;
    public static readonly Address[] Addresses;

    static UserSeeder()
    {
        var passwordHasher = new PasswordHasher<User>();
        var adminUser = new User
        {
            Id = 1,
            Name = "Admin User",
            Username = "admin",
            Email = "admin@admin",
            Cpf = "12345678901",
            Cellphone = "1234567890",
            MotherName = "Admin Mother",
            FatherName = "Admin Father",
            MotherCellphone = "0987654321",
            FatherCellphone = "1122334455",
            ApplicationRole = ApplicationRole.Administrator
        };

        var regularUser = new User
        {
            Id = 2,
            Name = "Regular User",
            Username = "user",
            Email = "user@user",
            Cpf = "98765432109",
            Cellphone = "9876543210",
            MotherName = "User Mother",
            FatherName = "User Father",
            MotherCellphone = "5566778899",
            FatherCellphone = "1122334455",
            ApplicationRole = ApplicationRole.User
        };

        adminUser.Password = passwordHasher.HashPassword(adminUser, "admin");
        regularUser.Password = passwordHasher.HashPassword(regularUser, "user");

        Users = new[] { adminUser, regularUser };

        Addresses = new[]
        {
            new Address
            {
                Id = 1,
                Street = "123 Admin St",
                City = "Admin City",
                State = "Admin State",
                PostalCode = "12345",
                Country = "Admin Country",
                UserId = 1
            },
            new Address
            {
                Id = 2,
                Street = "456 User St",
                City = "User City",
                State = "User State",
                PostalCode = "67890",
                Country = "User Country",
                UserId = 2
            }
        };
    }
}
