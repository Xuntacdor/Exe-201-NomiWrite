using FluentAssertions;
using FluentValidation;
using MassTransit;
using NSubstitute;
using NomiWrite.Admin.Application.DTOs;
using NomiWrite.Admin.Application.Services;
using NomiWrite.Admin.Application.Validation;
using NomiWrite.Shared.Contracts.Events.Admin;

namespace NomiWrite.Admin.Application.UnitTests;

public class AnnouncementServiceTests
{
    private static AnnouncementService Build(IPublishEndpoint? publish = null)
    {
        publish ??= Substitute.For<IPublishEndpoint>();
        return new AnnouncementService(publish, new AnnouncementRequestValidator());
    }

    private static List<SystemAnnouncementCreatedEvent> Events(IPublishEndpoint publish)
        => publish.ReceivedCalls()
            .SelectMany(c => c.GetArguments())
            .OfType<SystemAnnouncementCreatedEvent>()
            .ToList();

    #region U-AD2 — Announcement validation + event publish

    [Fact]
    public async Task Publish_ValidRequest_TrimsAndPublishesEvent()
    {
        var publish = Substitute.For<IPublishEndpoint>();
        var sut = Build(publish);

        await sut.PublishAnnouncementAsync(new AnnouncementRequestDto
        {
            Title = "  Maintenance window  ",
            Message = "  Service will be briefly offline at 02:00.  "
        });

        var events = Events(publish);
        events.Should().ContainSingle();
        events.Single().AnnouncementId.Should().NotBe(Guid.Empty);
        events.Single().Title.Should().Be("Maintenance window");
        events.Single().Message.Should().Be("Service will be briefly offline at 02:00.");
        events.Single().CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Publish_EmptyTitle_ThrowsAndDoesNotPublish()
    {
        var publish = Substitute.For<IPublishEndpoint>();
        var sut = Build(publish);

        var act = () => sut.PublishAnnouncementAsync(new AnnouncementRequestDto
        {
            Title = "",
            Message = "Hello"
        });

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Title is required*");
        publish.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public async Task Publish_MessageTooLong_Throws()
    {
        var publish = Substitute.For<IPublishEndpoint>();
        var sut = Build(publish);

        var act = () => sut.PublishAnnouncementAsync(new AnnouncementRequestDto
        {
            Title = "Title",
            Message = new string('x', 5001)
        });

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*5000*");
    }

    #endregion
}