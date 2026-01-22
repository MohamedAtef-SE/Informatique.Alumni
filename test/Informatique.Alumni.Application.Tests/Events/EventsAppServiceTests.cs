using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.PermissionManagement;
using Informatique.Alumni.Permissions;
using Xunit;

namespace Informatique.Alumni.Events;

public class EventsAppServiceTests : AlumniApplicationTestBase<AlumniApplicationTestModule>
{
    private readonly IEventsAppService _eventsAppService;
    private readonly IRepository<AssociationEvent, Guid> _eventRepository;
    private readonly Volo.Abp.Guids.IGuidGenerator _guidGenerator;
    private readonly Volo.Abp.Identity.IdentityUserManager _userManager;
    private readonly Volo.Abp.PermissionManagement.IPermissionManager _permissionManager;

    public EventsAppServiceTests()
    {
        _eventsAppService = GetRequiredService<IEventsAppService>();
        _eventRepository = GetRequiredService<IRepository<AssociationEvent, Guid>>();
        _guidGenerator = GetRequiredService<Volo.Abp.Guids.IGuidGenerator>();
        _userManager = GetRequiredService<Volo.Abp.Identity.IdentityUserManager>();
        _permissionManager = GetRequiredService<Volo.Abp.PermissionManagement.IPermissionManager>();
    }

    [Fact]
    public async Task GetAsync_Should_Load_Details_Optimized()
    {
        // Arrange
        var eventId = _guidGenerator.Create();
        var evt = new AssociationEvent(
            eventId,
            "Event Ar",
            "Event En",
            "EVT001",
            "Description",
            "Location",
            null,
            null,
            false,
            0,
            DateTime.Now.AddDays(10),
            null
        );
        
        evt.AddTimeslot(_guidGenerator.Create(), DateTime.Now.AddDays(1), DateTime.Now.AddDays(1).AddHours(2), 100);
        evt.AddAgendaItem(new EventAgendaItem(
            _guidGenerator.Create(),
            eventId,
            DateTime.Now.AddDays(1),
            TimeSpan.FromHours(9),
            TimeSpan.FromHours(10),
            "Keynote",
            "Main Hall",
            "Opening"
        ));
        
        await _eventRepository.InsertAsync(evt, autoSave: true);

        // Act
        var result = await _eventsAppService.GetAsync(eventId);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(eventId);
        result.Timeslots.Count.ShouldBeGreaterThan(0);
        result.AgendaItems.Count.ShouldBeGreaterThan(0);
        result.AgendaItems[0].ActivityName.ShouldBe("Keynote");
    }

    [Fact]
    public async Task GetMyRegistrationsAsync_Should_Bulk_Load_Event_Names()
    {
         // Arrange
        var userId = _guidGenerator.Create();
        var user = new Volo.Abp.Identity.IdentityUser(userId, "eventuser", "event@alumni.com");
        await _userManager.CreateAsync(user);
        
        // Grant Permissions
        await _permissionManager.SetForUserAsync(userId, AlumniPermissions.Events.Register, true);
        
        var eventId = _guidGenerator.Create();
        var evt = new AssociationEvent(
            eventId,
            "Tech Conference",
            "Tech Conference En",
            "TECH2026",
            "Desc",
            "Cairo",
            null,
            null,
            false,
            0,
            DateTime.Now.AddDays(5),
            null
        );
        evt.AddTimeslot(_guidGenerator.Create(), DateTime.Now.AddDays(1), DateTime.Now.AddDays(1).AddHours(2), 50);
        evt.Publish();
        
        await _eventRepository.InsertAsync(evt, autoSave: true);

        // Act - Register
        using (var scope = GetRequiredService<Volo.Abp.Security.Claims.ICurrentPrincipalAccessor>().Change(
            new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(
                    new[] { new System.Security.Claims.Claim(Volo.Abp.Security.Claims.AbpClaimTypes.UserId, userId.ToString()) }
                )
            )
        ))
        {
            await _eventsAppService.RegisterAsync(eventId);
            
            // Act - Get My Registrations
            var result = await _eventsAppService.GetMyRegistrationsAsync();
            
            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBeGreaterThan(0);
            result[0].EventId.ShouldBe(eventId);
            result[0].EventName.ShouldBe("Tech Conference En"); // Proves bulk loading worked
        }
    }
}
