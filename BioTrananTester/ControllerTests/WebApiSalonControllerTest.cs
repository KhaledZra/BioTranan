using BioTrananDomain.DtoModels;
using FluentAssertions;
using BioTransWebApi.Controllers;
using BioTrananDomain.Models;
using BioTransWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace BioTrananTester.ControllerTests;

public class WebApiSalonControllerTest : MongoDbIntegrationTest
{
    private readonly SalonService _salonService;

    public WebApiSalonControllerTest()
    {
        _salonService = new SalonService( 
            mongoDbService: new MongoDBService<Salon>(
                mongoClient: base.mongoClient,
                databaseName: "BioTrananTestingDb",
                collectionName: "Salon"));
    }
    
    
    // Salon Api Unit tests
    [Fact]
    public async Task POST_api_salon_should_add_salon()
    {
        // Arrange
        SalonController sut = new SalonController(_salonService);
        ActionResult<Salon> result = await sut.AddSalon(new SalonDto
        {
            SalonName = "Room A",
            SalonSeatAmount = 50
        });
        Salon addedSalon = (result.Result as CreatedAtActionResult).Value as Salon;

        // Act
        var action = await sut.GetSalon(addedSalon?.Id);
        var actionResult = action.Result as OkObjectResult;
        var foundSalon = actionResult?.Value as Salon;

        // Assert
        foundSalon?.Id.Should().Be(addedSalon.Id);
        foundSalon?.SalonName.Should().Be(addedSalon.SalonName);
        foundSalon?.SalonSeatAmount.Should().Be(addedSalon.SalonSeatAmount);
    }
    
    [Fact]
    public async Task PUT_api_salon_should_update_salon_on_success()
    {
        // Arrange
        SalonController sut = new SalonController(_salonService);
        ActionResult<Salon> result = await sut.AddSalon(new SalonDto
        {
            SalonName = "Room A",
            SalonSeatAmount = 50
        });
        Salon addedSalon = (result.Result as CreatedAtActionResult).Value as Salon;

        // Act
        var action = await sut.UpdateSalon(addedSalon.Id, new SalonDto
        {
            SalonName = "Room B",
            SalonSeatAmount = 25
        });
        var actionResult = action.Result as AcceptedAtActionResult;
        var salonValue = actionResult?.Value as Salon;
        
        // Assert
        salonValue.SalonName.Should().NotBe("Room A").And.Be("Room B");
        salonValue.SalonSeatAmount.Should().NotBe(50).And.Be(25);
    }
    
    [Fact]
    public async Task DELETE_api_salon_should_delete_salon_on_success()
    {
        // Arrange
        SalonController sut = new SalonController(_salonService);
        ActionResult<Salon> result = await sut.AddSalon(new SalonDto
        {
            SalonName = "Room A",
            SalonSeatAmount = 50
        });
        Salon addedSalon = (result.Result as CreatedAtActionResult).Value as Salon;

        // Act
        var action = await sut.DeleteSalon(addedSalon.Id);
        var actionResult = action.Result as NotFoundObjectResult;
        var salonValue = actionResult?.Value as Salon;
        
        // Assert
        salonValue.Should().BeNull();
    }
}