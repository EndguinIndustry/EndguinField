namespace EndguinField.Models.ViewModels
{
    public class IconButtonViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string IconPath { get; set; } = string.Empty;
        public string AltText { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string AdditionalClasses { get; set; } = string.Empty;
        public string OnClick { get; set; } = string.Empty;
        public bool Disabled { get; set; } = false;
    }
}
