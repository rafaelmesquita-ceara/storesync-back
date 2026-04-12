using System;
using System.IO;
using System.Text.RegularExpressions;

var basePath = @"C:\Users\Rafael\Downloads\CLAUDINHO\StoreSyncFront";
var files = Directory.GetFiles(basePath, "*.cs", SearchOption.AllDirectories);

foreach (var file in files)
{
    var originalContent = File.ReadAllText(file);
    var content = originalContent;

    // Handle ternary multiline and single line:
    // SnackBarService.Send(response.IsSuccess() ? "X" : "Y");
    content = Regex.Replace(content, 
        @"SnackBarService\.Send\(\s*response\.IsSuccess\(\)\s*\?\s*(.*?)\s*:\s*(.*?)\s*\);", 
        m => {
            return $"if (response.IsSuccess())\n            SnackBarService.SendSuccess({m.Groups[1].Value.Trim()});\n        else\n            SnackBarService.SendError({m.Groups[2].Value.Trim()});";
        }, 
        RegexOptions.Singleline);
    
    // Simple error rules
    content = Regex.Replace(content, @"SnackBarService\.Send\((\$?\""[^\"]*?)[Ee]rro(.*?)\""(.*?)?\);", "SnackBarService.SendError($1Erro$2\"$3);");
    
    // Simple success rules
    content = Regex.Replace(content, @"SnackBarService\.Send\((\$?\""[^\"]*?)sucesso(.*?)\""(.*?)?\);", "SnackBarService.SendSuccess($1sucesso$2\"$3);");

    // Warnings rules for particular words
    string[] warnTokens = { "Informe ", "informe ", "Selecione", "A senha", "O nome", "O login", "Apenas", "Data ", "Você näo", "Você não", "Usuário não", "Estoque", "O valor", "Os campos" };
    foreach (var t in warnTokens) 
    {
        content = Regex.Replace(content, $@"SnackBarService\.Send\((\$?\""[^\"]*?){t}(.*?)\""(.*?)?\);", $"SnackBarService.SendWarning($1{t}$2\"$3);");
    }

    if (content != originalContent)
    {
        File.WriteAllText(file, content);
        Console.WriteLine($"Fixed {Path.GetFileName(file)}");
    }
}
