using BusinessLogicLayer.Autofill;

namespace BusinessLogicLayer.Tests
{
    public class UnsupportedAutofillEngineTests
    {
        private readonly UnsupportedAutofillEngine sut = new();

        [Fact]
        public void IsSupported_ReturnsFalse()
        {
            Assert.False(sut.IsSupported);
        }

        [Fact]
        public void TriggerAutofill_AlwaysReturnsFalse()
        {
            Assert.False(sut.TriggerAutofill(1));
        }

        [Fact]
        public void RegisterHotkeys_DoesNotThrow()
        {
            var exception = Record.Exception(() => sut.RegisterHotkeys());

            Assert.Null(exception);
        }

        [Fact]
        public void UnregisterHotkeys_DoesNotThrow()
        {
            var exception = Record.Exception(() => sut.UnregisterHotkeys());

            Assert.Null(exception);
        }

        [Fact]
        public void HotkeyPressed_NeverInvoked()
        {
            var wasInvoked = false;
            sut.HotkeyPressed += () => wasInvoked = true;

            sut.RegisterHotkeys();
            sut.TriggerAutofill(1);

            Assert.False(wasInvoked);
        }
    }
}