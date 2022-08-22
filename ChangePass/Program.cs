// See https://aka.ms/new-console-template for more information
using Ionic.Zip;
using Ionic.Zlib;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Security;

try
{


string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pas.txt");

int i = 0;
string pasic;
string[] Data = new string[1];
    
    // Call the InitialSessionState.CreateDefault method to create
    // an empty InitialSessionState object, and then add the
    // elements that will be available when the runspace is opened.
    InitialSessionState iss = InitialSessionState.CreateDefault();

// Call the RunspaceFactory.CreateRunspace(InitialSessionState)
// method to create the runspace where the pipeline is run.
Runspace rs = RunspaceFactory.CreateRunspace(iss);
rs.Open();

// Call the PowerShell.Create() method to create the PowerShell object,
// and then specify the runspace and commands to the pipeline.
// and create the command pipeline.
PowerShell ps = PowerShell.Create();
PowerShell psOut = PowerShell.Create();
ps.Runspace = rs;

    using (StreamReader reader = new StreamReader(path))
    {
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            Data[i] = line;
            if (Data[i].Contains(@"\") == true)
            {
                Data[i] = line.Remove(0, 12);
            }

            Console.WriteLine(Data[i]);
            i++;
            Array.Resize(ref Data, i + 1);

        }
        Array.Resize(ref Data, i);
        Console.WriteLine(Data.Length);
    }

    using (StreamWriter writer = new StreamWriter(path, false))
    {
        for (i = 0; i < Data.Length; i++)
        {
            pasic = NewPas();
            ps.AddStatement();
            ps.AddCommand("Set-ADAccountPassword");
            ps.AddArgument(Data[i]);
            ps.AddParameter("-Reset");
            SecureString pas = new NetworkCredential("", pasic).SecurePassword;
            ps.AddParameter("-NewPassword", pas);
            ps.AddParameter("–PassThru");
            await writer.WriteLineAsync(Data[i] + ' ' + pasic);
        }
        Console.WriteLine();
        Console.WriteLine("WORK IN PROGRESS");
        ps.Invoke();
        Console.WriteLine("DONE");
    }

    Console.WriteLine();
    Console.WriteLine("PREPARING REPORT:");
    // Call the PowerShell.Invoke() method to run
    // the pipeline synchronously.
    for (i = 0; i < Data.Length; i++)
    {
        psOut.AddStatement().AddCommand("Get-ADUser").AddArgument(Data[i]).AddParameter("-properties", "PasswordLastSet");
    }
    foreach (PSObject result in psOut.Invoke())
    {
        Console.WriteLine("{0,-50}{1}",
                    result.Members["Name"].Value,
                    result.Members["PasswordLastSet"].Value);
    } // End foreach.

    // Close the runspace to free resources.
    rs.Close();

    using (var zip = new ZipFile())
    {
        // оставляем комментарий к архиву
        zip.Comment = "Archive created by " + "SAM" + " at " + DateTime.Now;

        // добавляем незапароленный файл в архив
        //zip.AddFile(path);

        // устанавливаем пароль к архиву
        zip.Password = "Qq1234567890!";

        // добавляем файл в архив, этот файл будет доступен только с вводом пароля, т.к. выше был установлен пароль
        zip.AddFile(path);

        // установка уровня сжатия
        zip.CompressionLevel = CompressionLevel.BestCompression;

        // сохраняем архив
        zip.Save("pas.zip");
    }
    string path2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Send.exe");
    System.Diagnostics.Process.Start(path2);
    Console.ReadKey();
}
catch (Exception)
{
    Console.WriteLine("FATAL SMERT ERROR");
}


string NewPas ()
{
    int a = 2;
    int b = 4;
    int c = 6;
    Random rnd = new Random();
    var upCase = Enumerable.Range(0, a).Select(t => (char)rnd.Next(65, 91)).ToArray();
    var lowCase = Enumerable.Range(0, b).Select(t => (char)rnd.Next(97, 123)).ToArray();
    var digit = Enumerable.Range(0, c).Select(t => (char)rnd.Next(48, 58)).ToArray();
    string password = string.Join("", upCase.Concat(lowCase).Concat(digit).OrderBy(t => Guid.NewGuid()));
    return password;
}