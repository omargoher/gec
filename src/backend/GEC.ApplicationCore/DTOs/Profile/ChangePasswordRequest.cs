namespace GEC.ApplicationCore.DTOs.Profile;

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword);