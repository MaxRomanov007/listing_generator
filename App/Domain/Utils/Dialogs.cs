using System.Threading.Tasks;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;

namespace App.Domain.Utils;

public static class Dialogs
{
    public static async Task ShowErrorAsync(string message)
    {
        await MessageBoxManager.GetMessageBoxStandard(
            new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.Ok,
                ContentTitle = "Ошибка",
                ContentMessage = message,
                Icon = Icon.Error,
                WindowIcon = new WindowIcon("Assets/Logo.ico"),
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            }
        ).ShowAsync();
    }

    public static async Task<ButtonResult> ShowQuestionAsync(string question)
    {
        return await MessageBoxManager.GetMessageBoxStandard(
            new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.YesNo,
                ContentTitle = "Подтверждение",
                ContentMessage = question,
                Icon = Icon.Question,
                WindowIcon = new WindowIcon("Assets/Logo.ico"),
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            }
        ).ShowAsync();
    }

    public static async Task ShowSuccessAsync(string message)
    {
        await MessageBoxManager.GetMessageBoxStandard(
            new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.Ok,
                ContentTitle = "Выполнено",
                ContentMessage = message,
                Icon = Icon.Success,
                WindowIcon = new WindowIcon("Assets/Logo.ico"),
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            }
        ).ShowAsync();
    }
}