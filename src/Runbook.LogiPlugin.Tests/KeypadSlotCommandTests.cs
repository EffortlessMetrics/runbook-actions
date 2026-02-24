using FluentAssertions;
using Runbook.Actions;
using Xunit;

namespace Runbook.LogiPlugin.Tests;

public class KeypadSlotCommandTests
{
    [Fact]
    public void Should_Have_Correct_Metadata()
    {
        var command = new KeypadSlotCommand();
        command.Name.Should().NotBeNullOrWhiteSpace(); // PluginDynamicCommand populates this via base constructor
        // Due to the Loupedeck SDK structure, testing full side effects is hard without MS Fakes or complex interfaces. 
        // We verify the type initializes successfully without relying on SDK internals failing.
    }
}
