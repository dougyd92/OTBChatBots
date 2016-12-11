using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

// For more information about this template visit http://aka.ms/azurebots-csharp-basic
[Serializable]
public class EchoDialog : IDialog<object>
{
    protected int count = 1;

    public Task StartAsync(IDialogContext context)
    {
        try
        {
            context.Wait(MessageReceivedAsync);
        }
        catch (OperationCanceledException error)
        {
            return Task.FromCanceled(error.CancellationToken);
        }
        catch (Exception error)
        {
            return Task.FromException(error);
        }

        return Task.CompletedTask;
    }

    public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
    {
        var message = await argument;
        if (message.Text == "reset")
        {
            PromptDialog.Confirm(
                context,
                AfterResetAsync,
                "Are you sure you want to reset the count?",
                "Didn't get that!",
                promptStyle: PromptStyle.Auto);
        }
        else if (message.Text.ToUpper().Contains("ARRAY"))
        {
            await context.PostAsync("I will attempt to parse your message into an array.");

            List<string> options = message.Text.Split(' ').ToList();

            PromptDialog.Choice(
                context,
                AfterArrayChoiceAsync,
                options,
                "Which would you like to choose ?",
                "I'm sorry, I didn't understand your selection.",
                5,
                PromptStyle.Auto
                );
        }
        else
        {
            await context.PostAsync($"{this.count++}: You said {message.Text.ToUpper()}");
            context.Wait(MessageReceivedAsync);
        }
    }

    public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;
        if (confirm)
        {
            this.count = 1;
            await context.PostAsync("Reset count.");
        }
        else
        {
            await context.PostAsync("Did not reset count.");
        }
        context.Wait(MessageReceivedAsync);
    }

    public async Task AfterArrayChoiceAsync(IDialogContext context, IAwaitable<string> argument)
    {
        var option = await argument;

        await context.PostAsync("Array Aysnc handler.");
        
        context.Wait(MessageReceivedAsync);
    }
    
}
