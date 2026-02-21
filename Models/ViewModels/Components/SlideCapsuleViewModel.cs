namespace EndguinField.Models.ViewModels;

public class SlideCapsuleViewModel
{
    public string CurrentValue { get; set; } = string.Empty;
    public string GroupName { get; set; } = "slideCapsule";
    public List<(string Value, string Variant)> Options { get; set; } = new();
}
