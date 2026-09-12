using System.Data;
using Moq;

namespace App.Api.Tests.Repositories;

public class OrderRepositoryTests
{
    // [Fact]
    // public void Constructor_CreatesRepositoryInstance()
    // {
    //     // Arrange
    //     var dbConnection = new Mock<IDbConnection>();

    //     // Act
    //     var repository = new OrdersRepository(dbConnection.Object);

    //     // Assert
    //     Assert.NotNull(repository);
    //     Assert.IsAssignableFrom<IOrdersRepository>(repository);
    // }

    // [Fact]
    // public void IOrdersRepository_Get_ReturnType_IsTaskOfNullableString()
    // {
    //     // Act
    //     var method = typeof(IOrdersRepository).GetMethod(nameof(IOrdersRepository.Get));

    //     // Assert
    //     Assert.NotNull(method);
    //     Assert.Equal(typeof(Task<string?>), method!.ReturnType);
    // }
}