using ATS.Application.Abstract.AI;
using ATS.Infrastructure.Concrete.AI;
using ATS.Tests.TestSupport;

namespace ATS.Tests
{
    public class SelectedProviderChatModelIdentityProviderTests
    {
        private static SelectedProviderChatModelIdentityProvider CreateProvider(AiProviderSelection selection) =>
            new(new AiProviderClientResolver(selection, new IAiProviderClientFactory[]
            {
                new FakeAiProviderClientFactory("First", "First:chat-model"),
                new FakeAiProviderClientFactory("Second", "Second:chat-model")
            }));

        [Fact]
        public void GetChatModelIdentity_ReturnsTheIdentityOfTheSelectedProvider()
        {
            var selection = new AiProviderSelection();
            selection.Select("Second");

            Assert.Equal("Second:chat-model", CreateProvider(selection).GetChatModelIdentity());
        }

        [Fact]
        public void GetChatModelIdentity_Throws_WhenNoProviderHasBeenSelected()
        {
            Assert.Throws<InvalidOperationException>(() => CreateProvider(new AiProviderSelection()).GetChatModelIdentity());
        }
    }
}
