

namespace ATS.Application.Abstract.AI
{
    public class AiProviderSelection
    {
        public string? SelectedProviderName { get; private set; }

        public void Select(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName))
                throw new ArgumentException("Provider name must not be blank.", nameof(providerName));

            SelectedProviderName = providerName;
        }

        public void Clear()
        {
            SelectedProviderName = null;
        }
    }
}
