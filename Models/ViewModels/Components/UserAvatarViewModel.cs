namespace EndguinField.Models.ViewModels.Components
{
    public class UserAvatarViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string CssClass { get; set; } = string.Empty;
        public string? AltText { get; set; }
    }
}
