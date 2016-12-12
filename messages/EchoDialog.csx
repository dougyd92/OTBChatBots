using System;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

using QC = System.Data.SqlClient;
using DT = System.Data;

// For more information about this template visit http://aka.ms/azurebots-csharp-basic
[Serializable]
public class EchoDialog : IDialog<object>
{
    private string searchstring;
    private int current_result;
    private bool out_of_rows;

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
        if ( (message.Text.ToUpper().Contains("DIAGNOSE")) ||
             (message.Text.ToUpper().Contains("PROBLEM")) ||
             (message.Text.ToUpper().Contains("HELP")) ||
             (message.Text.ToUpper().Contains("ISSUE")))
        {
              PromptDialog.Text(
                  context,
                  AfterGetSearchStringAsync,
                  "I'm sure I can help with that! \n\rPlease give a detailed description of your problem.",
                  "I'm sorry, I didn't understand that.",
                  5
                  ); 
            
        }
        else if ((message.Text.ToUpper().Contains("SUCKS")) ||
                 (message.Text.ToUpper().Contains("SLOW")) ||
                 (message.Text.ToUpper().Contains("OVER IT")) ||
                 (message.Text.ToUpper().Contains("TERRIBLE")) ||
                 (message.Text.ToUpper().Contains("BACK TO EXCEL")))
        {
            await context.PostAsync("I'm sorry you are not 100% satisfied with our software. Have you considered upgrading?\n\rThe latest version of Alliant is Alliant 6.6, which was released on 9/16/16.");

            PromptDialog.Confirm(
                context,
                NewVersionFeaturesAsync,
                "Would you like to hear about the new features in 6.6?",
                "I couldn't understand your choice.",
                promptStyle: PromptStyle.Auto);

        }
        else if ((message.Text.ToUpper().Contains("VERSION")) ||
                 (message.Text.ToUpper().Contains("UPGRAD")) ||
                 (message.Text.ToUpper().Contains("LATEST")) ||
                 (message.Text.ToUpper().Contains("WHAT'S NEW")) ||
                 (message.Text.ToUpper().Contains("NEWEST")))
        {
            await context.PostAsync("It's great that you are interested in staying up-to-date!\n\rThe latest version of Alliant is Alliant 6.6, which was released on 9/16/16.");

            PromptDialog.Confirm(
                context,
                NewVersionFeaturesAsync,
                "Would you like to hear about the new features in 6.6?",
                "I couldn't understand your choice.",
                promptStyle: PromptStyle.Auto);

        }
        else if ((message.Text.ToUpper().Contains("REAL PERSON")) ||
         (message.Text.ToUpper().Contains("HUMAN")) ||
         (message.Text.ToUpper().Contains("HUMAN BEING")) ||
         (message.Text.ToUpper().Contains("PHONE NUMBER")))
        {
            await context.PostAsync(@"I am happy to put you in touch with a represntative! I'm sorry I couldn't resolve your issue; I am still a prototype, after all.");
            await ListContactInfoAsync(context);
            context.Wait(MessageReceivedAsync);
        }
        else
        {
            await context.PostAsync(@"I'm sorry, I didn't understand that. You can say things like ""Diagnose a problem"" or ""Tell me about the lastest version of Alliant."" ");
            context.Wait(MessageReceivedAsync);
        }
    }

    public async Task AfterGetSearchStringAsync(IDialogContext context, IAwaitable<string> argument)
    {
        var s = await argument;

        searchstring = s;
        current_result = 1;
        out_of_rows = false;

        DBStuff dbconnection = new DBStuff();

        await dbconnection.connect_to_db(context);

        bool rows_found = await dbconnection.lookup_troubleshooting(searchstring, current_result, context);

        if (rows_found)
            PromptDialog.Confirm(
                    context,
                    DidTSGSolve,
                    "Did that solve your issue?",
                    "I couldn't understand your choice.",
                    promptStyle: PromptStyle.Auto);
        else
        {
            await context.PostAsync(@"It looks like I was not able to help.");
            context.Wait(MessageReceivedAsync);
        }

    }

    public async Task ListContactInfoAsync(IDialogContext context)
    {
        IMessageActivity newMessage = context.MakeMessage();

        List<CardAction> cardButtons = new List<CardAction>();
        CardAction emailbutton = new CardAction()
        {
            Value = "mailto:AlliantSupport@realsoftwaresystems.com",
            Type = "openUrl",
            Title = "AlliantSupport@realsoftwaresystems.com"
        };

        cardButtons.Add(emailbutton);

        CardAction phonebutton = new CardAction()
        {
            Value = "8183138060",
            Type = "call",
            Title = "(818) 313 - 8060"
        };

        cardButtons.Add(phonebutton);

        List<CardImage> cardImages = new List<CardImage>();
        cardImages.Add(new CardImage(url: "http://www.realsoftwaresystems.com/sites/all/themes/realsoft/logo.png"));

        HeroCard plCard = new HeroCard()
        {
            Title = "Alliant Customer Support",
            Images = cardImages,
            Buttons = cardButtons
        };


        Attachment plAttachment = plCard.ToAttachment();
        List<Attachment> attachment_list = new List<Attachment>();
        attachment_list.Add(plAttachment);
        newMessage.Attachments = attachment_list; //issue

        await context.PostAsync(newMessage);
    }

    public async Task NewVersionFeaturesAsync(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;
        
        if (confirm)
        {
            DBStuff dbconnection = new DBStuff();
            await dbconnection.connect_to_db(context);

            await dbconnection.print_new_features(context);
            System.Threading.Thread.Sleep(1500);
            await context.PostAsync(@"There sure were a lot of great features in there!");
        }
        else
        {
            await context.PostAsync(@"Fine then.");
        }
        System.Threading.Thread.Sleep(1000);
        PromptDialog.Confirm(
            context,
            NewVersionFixedIssuesAsync,
            "How about the fixed issues? Maybe the problem you are experiencing has been fixed already!",
            "I couldn't understand your choice.",
            promptStyle: PromptStyle.Auto);
    }

    public async Task NewVersionFixedIssuesAsync(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;

        if (confirm)
        {
            DBStuff dbconnection = new DBStuff();
            await dbconnection.connect_to_db(context);

            await dbconnection.print_fixed_issues(context);

            await context.PostAsync(@"Wow, it looks like the new version is really a step up!");
        }
        else
        {
            await context.PostAsync(@"That's fine.");
        }

        System.Threading.Thread.Sleep(1500);

        await context.PostAsync(@"If you are interested in upgrading, please get in touch with us by calling the number below!");
        await ListContactInfoAsync(context);

        context.Wait(MessageReceivedAsync);
    }

    public async Task DidTSGSolve(IDialogContext context, IAwaitable<bool> argument)
    {
        var confirm = await argument;

        if (confirm)
        {
            await context.PostAsync(@"I'm glad that we solved that so fast!");
            context.Wait(MessageReceivedAsync);
        }
        else
        {
            //Get the next result
            DBStuff dbconnection = new DBStuff();

            await dbconnection.connect_to_db(context);

            bool rows_found = await dbconnection.lookup_troubleshooting(searchstring, ++current_result, context);

            if (rows_found)
            {
                await context.PostAsync(@"Let's try again.");

                PromptDialog.Confirm(
                        context,
                        DidTSGSolve,
                        "Did that solve your issue?",
                        "I couldn't understand your choice.",
                        promptStyle: PromptStyle.Auto);
            }
            else
            {
                await context.PostAsync(@"It looks like I was not able to help.");
                context.Wait(MessageReceivedAsync);
            }
        }
        
    }
}

class DBStuff
{ 
    private QC.SqlConnection connection;

    public async Task connect_to_db(IDialogContext context)
    {
        connection = new QC.SqlConnection(
        "Server=tcp:vsddj1.database.windows.net,1433;Initial Catalog=AlliantSupport;Persist Security Info=False;User ID=ddejesus;Password=t1mrssACC;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
        );

        try
        {
            connection.Open();
        }
        catch (InvalidOperationException e)
        {
            await context.PostAsync("Caught InvalidOperationException while conencting to DB.");
            return;
        }
        catch (ConfigurationErrorsException e)
        {
            await context.PostAsync("Caught ConfigurationErrorsException while conencting to DB.");
            return;
        }
        catch (QC.SqlException ex)
        {
            for (int i = 0; i < ex.Errors.Count; i++)
            {
                await context.PostAsync("Index #" + i + "\n" +
                    "Message: " + ex.Errors[i].Message + "\n" +
                    "LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                    "Source: " + ex.Errors[i].Source + "\n" +
                    "Procedure: " + ex.Errors[i].Procedure + "\n");
            }
            return;

        }
        
    }

    public async Task<bool> lookup_troubleshooting(string searchstring, int row_num, IDialogContext context)
    {
        bool rows_found;
        using (var command = new QC.SqlCommand())
        {
            command.Connection = connection;
            command.CommandType = DT.CommandType.Text;
            command.CommandText = @"  
                    SELECT summary, full_description
                    FROM (
                        SELECT summary, full_description, ROW_NUMBER() OVER (ORDER BY KEY_TBL.rank, T.row_id) as row_num
                        FROM troubleshooting T
                        INNER JOIN FREETEXTTABLE(troubleshooting, summary, @searchstring) KEY_TBL ON T.row_id = KEY_TBL.[KEY]
                        ) as ordered_and_numbered
                    WHERE row_num = @row_to_get;";
            command.Parameters.Add("@searchstring", DT.SqlDbType.NVarChar).Value = searchstring;
            command.Parameters.Add("@row_to_get", DT.SqlDbType.Int).Value = row_num;
            QC.SqlDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                String s = String.Format("**Issue: {0}**\n\rSolution: {1}", reader.GetString(0), reader.GetString(1));
                await context.PostAsync(s);
                rows_found = true;
            }
            else
            {
                String s;
                if (row_num > 1)
                    s = "I couldn't find any more results.";
                else
                    s = "I couldn't find any results.";
                
                await context.PostAsync(s);
                rows_found = false;
            }
        }
        
        return rows_found;
    }
    public async Task print_new_features(IDialogContext context)
    {
        using (var command = new QC.SqlCommand())
        {
            command.Connection = connection;
            command.CommandType = DT.CommandType.Text;
            command.CommandText = @"  
                    SELECT title, description
                    FROM release_notes_new_featues; ";
            QC.SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                String s = String.Format("**{0}**\n\r{1}", reader.GetString(0), reader.GetString(1));
                await context.PostAsync(s);
                System.Threading.Thread.Sleep(500);
            }
        }
    }

    public async Task print_fixed_issues(IDialogContext context)
    {
        using (var command = new QC.SqlCommand())
        {
            command.Connection = connection;
            command.CommandType = DT.CommandType.Text;
            command.CommandText = @"  
                    SELECT title, description
                    FROM release_notes_fixed_issues; ";
            QC.SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                String s = String.Format("**{0}**\n\r{1}", reader.GetString(0), reader.GetString(1));
                await context.PostAsync(s);
                System.Threading.Thread.Sleep(500);
            }
        }
    }
}
