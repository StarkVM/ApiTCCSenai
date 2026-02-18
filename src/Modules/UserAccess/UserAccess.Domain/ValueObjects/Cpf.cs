namespace UserAccess.Domain.ValueObjects;

using System.Linq;

public sealed class Cpf
{
    public string Value { get; private set; }

    public Cpf(string value)
    {
        var cleaned = Clean(value);

        if (!IsValid(cleaned))
        {
            throw new ArgumentException("Invalid CPF");
        }
        
        Value = cleaned;
    }
    
    private static string Clean(string cpf) => cpf.Replace(".", "").Replace("-", "").Replace(" ", "");

    private static bool IsValid(string cpf)
    {
        if (cpf.Length != 11){ return false;}
        if (cpf.All(c => c == cpf[0])){ return false;}

        int[] mult1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] mult2 = { 11,10, 9, 8, 7, 6, 5, 4, 3, 2 };

        string temp = cpf[..9];
        int sum = 0;

        for (int i = 0; i < 9; i++)
        {
            sum += (temp[i] - '0') * mult1[i];
        }
        
        int rem = sum % 11;
        
        rem = rem < 2 ? 0 : 11 - rem;

        temp += rem.ToString();

        sum = 0;
        for (int i = 0; i < 10; i++)
            sum += (temp[i] - '0') * mult2[i];

        rem = sum % 11;
        rem = rem < 2 ? 0 : 11 - rem;

        var dv = cpf[9].ToString() + cpf[10].ToString();
        var calc = temp[9].ToString() + rem.ToString();

        return dv == calc;
    }
    
}