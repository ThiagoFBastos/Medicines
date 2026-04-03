using Medicines.Enums;
using Medicines.Interfaces;
using Medicines.Models;
using Medicines.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace MedicinesTests
{
    public class UserServiceTest
    {
        private readonly Mock<IRepositoryManager> _repositoryManager;
        private readonly UserService _userService;

        public UserServiceTest()
        {
            ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());

            var logger = factory.CreateLogger<UserService>();

            _repositoryManager = new Mock<IRepositoryManager>();

            _userService = new UserService(_repositoryManager.Object, logger);
        }


        [Fact]
        public async Task AddUserAsyncUserAlreadyExistsTest()
        {
            var userRepository = new Mock<IUserRepository>();

            const long userId = 1;
            const string username = "testuser";

            var user = new User { UserId = userId, Username = username };

            userRepository.Setup(u => u.GetUserByUserIdAsync(It.IsAny<long>())).ReturnsAsync(user);
            _repositoryManager.SetupGet(r => r.UserRepository).Returns(userRepository.Object);

            var result = await _userService.AddUserAsync(userId, username);

            Assert.False(result.IsSuccess);

            var error = result.Error;

            Assert.Equal(EUserStatusCode.USER_ALREADY_EXISTS, error);
        }

        [Fact]
        public async Task AddUserNotValidTest()
        {
            var userRepository = new Mock<IUserRepository>();

            const long userId = 1;
            const string username = "$$$";

            userRepository.Setup(u => u.GetUserByUserIdAsync(It.IsAny<long>())).ReturnsAsync((User?)null);
            _repositoryManager.SetupGet(r => r.UserRepository).Returns(userRepository.Object);

            var result = await _userService.AddUserAsync(userId, username);

            Assert.False(result.IsSuccess);

            var error = result.Error;

            Assert.Equal(EUserStatusCode.USER_DATA_INVALID, error);
        }

        [Fact]
        public async Task AddUserAsyncSuccessTest()
        {
            var userRepository = new Mock<IUserRepository>();
            const long userId = 1;
            const string username = "testuser";

            userRepository.Setup(u => u.GetUserByUserIdAsync(It.IsAny<long>())).ReturnsAsync((User?)null);
            userRepository.Setup(u => u.AddUser(It.IsAny<User>())).Verifiable();

            _repositoryManager.SetupGet(r => r.UserRepository).Returns(userRepository.Object);
            _repositoryManager.Setup(r => r.SaveAsync()).Verifiable();

            var result = await _userService.AddUserAsync(userId, username);

            userRepository.VerifyAll();
            _repositoryManager.VerifyAll();

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task AddUserAddErrorTest()
        {
            var userRepository = new Mock<IUserRepository>();
            const long userId = 1;
            const string username = "testuser";

            userRepository.Setup(u => u.GetUserByUserIdAsync(It.IsAny<long>())).ReturnsAsync((User?)null);
            userRepository.Setup(u => u.AddUser(It.IsAny<User>())).Verifiable();

            _repositoryManager.SetupGet(r => r.UserRepository).Returns(userRepository.Object);
            _repositoryManager.Setup(r => r.SaveAsync()).ThrowsAsync(new Exception()).Verifiable();

            var result = await _userService.AddUserAsync(userId, username);

            userRepository.VerifyAll();
            _repositoryManager.VerifyAll();

            Assert.False(result.IsSuccess);
            Assert.Equal(EUserStatusCode.USER_ADD_ERROR, result.Error);
        }

        [Fact]
        public async Task DeleteUserAsyncUserNotFoundTest()
        {
            var userRepository = new Mock<IUserRepository>();
            const long userId = 1;

            userRepository.Setup(u => u.GetUserByUserIdAsync(It.IsAny<long>())).ReturnsAsync((User?)null);

            _repositoryManager.SetupGet(r => r.UserRepository).Returns(userRepository.Object);

            var result = await _userService.DeleteUserAsync(userId);

            Assert.False(result.IsSuccess);

            Assert.Equal(EUserStatusCode.USER_NOT_FOUND, result.Error);
        }

        [Fact]
        public async Task DeleteUserAsyncSuccessTest()
        {
            var userRepository = new Mock<IUserRepository>();
            const long userId = 1;
            const string username = "testuser";

            var user = new User { UserId = userId, Username = username };

            userRepository.Setup(u => u.GetUserByUserIdAsync(It.IsAny<long>())).ReturnsAsync(user);
            userRepository.Setup(u => u.DeleteUser(It.IsAny<User>())).Verifiable();

            _repositoryManager.SetupGet(r => r.UserRepository).Returns(userRepository.Object);
            _repositoryManager.Setup(r => r.SaveAsync()).Verifiable();

            var result = await _userService.DeleteUserAsync(userId);

            Assert.True(result.IsSuccess);

            userRepository.VerifyAll();
            _repositoryManager.VerifyAll();
        }

        [Fact]
        public async Task DeleteUserAsyncDeleteErrorTest()
        {
            var userRepository = new Mock<IUserRepository>();
            const long userId = 1;
            const string username = "testuser";

            var user = new User { UserId = userId, Username = username };

            userRepository.Setup(u => u.GetUserByUserIdAsync(It.IsAny<long>())).ReturnsAsync(user);
            userRepository.Setup(u => u.DeleteUser(It.IsAny<User>())).Verifiable();

            _repositoryManager.SetupGet(r => r.UserRepository).Returns(userRepository.Object);
            _repositoryManager.Setup(r => r.SaveAsync()).Throws(new Exception()).Verifiable();

            var result = await _userService.DeleteUserAsync(userId);

            Assert.False(result.IsSuccess);
            Assert.Equal(EUserStatusCode.USER_DELETE_ERROR, result.Error);

            userRepository.VerifyAll();
            _repositoryManager.VerifyAll();
        }

        [Fact]
        public async Task GetAllUsersAsyncSuccessTest()
        {
            var userRepository = new Mock<IUserRepository>();

            var users = new List<User>
            {
                new User { UserId = 1, Username = "testuser1" },
                new User { UserId = 2, Username = "testuser2" }
            };

            userRepository.Setup(u => u.GetAllUsersAsync()).ReturnsAsync(users);
            _repositoryManager.SetupGet(r => r.UserRepository).Returns(userRepository.Object);

            var result = await _userService.GetAllUsersAsync();

            Assert.True(result.IsSuccess);
            Assert.Equal(users, result.Value);
        }

        [Fact]
        public async Task GetAllUsersAsyncGetErrorTest()
        {
            var userRepository = new Mock<IUserRepository>();

            userRepository.Setup(u => u.GetAllUsersAsync()).ThrowsAsync(new Exception());
            _repositoryManager.SetupGet(r => r.UserRepository).Returns(userRepository.Object);

            var result = await _userService.GetAllUsersAsync();

            Assert.False(result.IsSuccess);
            Assert.Equal(EUserStatusCode.USER_LIST_ERROR, result.Error);
        }

        [Fact]
        public async Task GetUserByUserIdAsyncUserNotFoundTest()
        {
            var userRepository = new Mock<IUserRepository>();
            const long userId = 1;

            userRepository.Setup(u => u.GetUserByUserIdAsync(It.IsAny<long>())).ReturnsAsync((User?)null);
            _repositoryManager.SetupGet(r => r.UserRepository).Returns(userRepository.Object);

            var result = await _userService.GetUserByUserIdAsync(userId);

            Assert.False(result.IsSuccess);
            Assert.Equal(EUserStatusCode.USER_NOT_FOUND, result.Error);
        }

        [Fact]
        public async Task GetUserByUserIdAsyncSuccessTest()
        {
            var userRepository = new Mock<IUserRepository>();
            const long userId = 1;
            const string username = "testuser";
            var user = new User { UserId = userId, Username = username };

            userRepository.Setup(u => u.GetUserByUserIdAsync(It.IsAny<long>())).ReturnsAsync(user);
            _repositoryManager.SetupGet(r => r.UserRepository).Returns(userRepository.Object);

            var result = await _userService.GetUserByUserIdAsync(userId);

            Assert.True(result.IsSuccess);
            Assert.Equal(user, result.Value);
        }

        [Fact]
        public async Task GetUserByUserIdAsyncUserGetErrorTest()
        {
            var userRepository = new Mock<IUserRepository>();
            const long userId = 1;

            userRepository.Setup(u => u.GetUserByUserIdAsync(It.IsAny<long>())).ThrowsAsync(new Exception());
            _repositoryManager.SetupGet(r => r.UserRepository).Returns(userRepository.Object);

            var result = await _userService.GetUserByUserIdAsync(userId);

            Assert.False(result.IsSuccess);
            Assert.Equal(EUserStatusCode.USER_GET_ERROR, result.Error);
        }

        [Fact]
        public async Task UpdateUserAsyncUserNotFoundTest()
        {
            var userRepository = new Mock<IUserRepository>();
            const long userId = 1;
            const string username = "testuser";

            userRepository.Setup(u => u.GetUserByUserIdAsync(It.IsAny<long>())).ReturnsAsync((User?)null);
            _repositoryManager.SetupGet(r => r.UserRepository).Returns(userRepository.Object);

            var result = await _userService.UpdateUserAsync(userId, username);

            Assert.False(result.IsSuccess);
            Assert.Equal(EUserStatusCode.USER_NOT_FOUND, result.Error);
        }

        [Fact]
        public async Task UpdateUserAsyncInvalidUserTest()
        {
            var userRepository = new Mock<IUserRepository>();
            const long userId = 1;
            const string username = "$$$";
            var user = new User { UserId = userId, Username = "testuser" };

            userRepository.Setup(u => u.GetUserByUserIdAsync(It.IsAny<long>())).ReturnsAsync(user);
            _repositoryManager.SetupGet(r => r.UserRepository).Returns(userRepository.Object);

            var result = await _userService.UpdateUserAsync(userId, username);

            Assert.False(result.IsSuccess);
            Assert.Equal(EUserStatusCode.USER_DATA_INVALID, result.Error);
        }

        [Fact]
        public async Task UpdateUserAsyncSuccessTest()
        {
            var userRepository = new Mock<IUserRepository>();
            const long userId = 1;
            const string username = "testuser";
            const string newUsername = "newtestuser";
            var user = new User { UserId = userId, Username = username };

            userRepository.Setup(u => u.GetUserByUserIdAsync(It.IsAny<long>())).ReturnsAsync(user);
            userRepository.Setup(u => u.UpdateUser(It.IsAny<User>())).Verifiable();

            _repositoryManager.SetupGet(r => r.UserRepository).Returns(userRepository.Object);
            _repositoryManager.Setup(r => r.SaveAsync()).Verifiable();

            var result = await _userService.UpdateUserAsync(userId, newUsername);

            Assert.True(result.IsSuccess);

            userRepository.VerifyAll();
            _repositoryManager.VerifyAll();
        }

        [Fact]
        public async Task UpdateUserAsyncUpdateErrorTest()
        {
            var userRepository = new Mock<IUserRepository>();
            const long userId = 1;
            const string username = "testuser";
            const string newUsername = "newtestuser";
            var user = new User { UserId = userId, Username = username };

            userRepository.Setup(u => u.GetUserByUserIdAsync(It.IsAny<long>())).ReturnsAsync(user);
            userRepository.Setup(u => u.UpdateUser(It.IsAny<User>())).Verifiable();

            _repositoryManager.SetupGet(r => r.UserRepository).Returns(userRepository.Object);
            _repositoryManager.Setup(r => r.SaveAsync()).ThrowsAsync(new Exception()).Verifiable();

            var result = await _userService.UpdateUserAsync(userId, newUsername);

            Assert.False(result.IsSuccess);
            Assert.Equal(EUserStatusCode.USER_UPDATE_ERROR, result.Error);

            userRepository.VerifyAll();
            _repositoryManager.VerifyAll();

        }
    }
}