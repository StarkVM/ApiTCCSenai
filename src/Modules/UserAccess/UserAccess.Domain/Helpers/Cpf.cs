namespace UserAccess.Domain.Helpers;

public static class Cpf
{
    public static bool CpfIsValid(this string cpf)
    {

        cpf = cpf.Clean();
        
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
        {
            sum += (temp[i] - '0') * mult2[i];
        }
        
        rem = sum % 11;
        rem = rem < 2 ? 0 : 11 - rem;

        var dv = cpf[9].ToString() + cpf[10].ToString();
        var calc = temp[9].ToString() + rem.ToString();

        return dv == calc;
    }

    public static string Clean(this string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
        {
            return string.Empty;
        }
        return cpf = new string(cpf.Where(char.IsDigit).ToArray());
    }
    
}