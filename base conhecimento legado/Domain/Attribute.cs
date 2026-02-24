using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace Domain
{
    public class LowerCaseAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            try
            {
                validationContext
                .ObjectType
                .GetProperty(validationContext.DisplayName)
                    ?.SetValue(validationContext.ObjectInstance, value.ToString().ToLower(), null);
            }
            catch (System.Exception)
            {
            }
            return IsValid(value) ? null : new ValidationResult(this.ErrorMessage);
        }
    }
    public class UpperCaseAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            try
            {
                validationContext
                .ObjectType
                .GetProperty(validationContext.DisplayName)
                    ?.SetValue(validationContext.ObjectInstance, value.ToString().ToUpper(), null);
            }
            catch (Exception)
            {
            }
            return null;
        }
    }
    public class MinimoItensListaAttribute : ValidationAttribute
    {
        private readonly int _minElements;
        public MinimoItensListaAttribute(int minElements)
        {
            _minElements = minElements;
        }

        public override bool IsValid(object value)
        {
            if (value is IList list)
            {
                return list.Count >= _minElements;
            }
            return false;
        }
    }
    public class RequiredTipo : ValidationAttribute
    {
        private readonly string _tipo;

        public RequiredTipo(string tipo)
        {
            _tipo = tipo;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var resultado = false;
            try
            {
                var tipo = validationContext.ObjectType.GetProperty("TIPO")?.GetValue(validationContext.ObjectInstance, null).ToString();
                if (tipo == _tipo)
                {
                    if (value == null) return new ValidationResult(ErrorMessage);
                    var stringValue = value as string;
                    if (!string.IsNullOrWhiteSpace(stringValue))
                    {
                        return stringValue.Trim().Length != 0 ? null : new ValidationResult(ErrorMessage);
                    }
                }
                resultado = true;
            }
            catch (Exception)
            {

            }
            return resultado ? null : new ValidationResult(ErrorMessage);
        }
    }
    public class TotalPagamentosAttribute : ValidationAttribute
    {
        private readonly string _propriedadelista;
        private readonly string _campo;

        public TotalPagamentosAttribute(string propriedadelista, string campo)
        {
            _propriedadelista = propriedadelista;
            _campo = campo;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var resultado = false;
            try
            {
                var lista =
                    validationContext.ObjectType.GetProperty(_propriedadelista)
                        ?.GetValue(validationContext.ObjectInstance, null);

                if ((int)lista.GetType().GetProperty("Count").GetValue(lista, null) > 0)
                {
                    resultado = Math.Abs(((IEnumerable)lista).Cast<object>().Aggregate(0.0, (current, item) => current + (double)item.GetType().GetProperty(_campo).GetValue(item, null)) - (double)value) < 0.01;
                }
                else
                    if (Math.Abs((double)value) < 0.01) resultado = true;

            }
            catch (Exception)
            {

            }

            return resultado ? null : new ValidationResult(this.ErrorMessage);
        }

    }

    public class ValidarCpfCnpjAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return IsCpfcnpj((string)value, ErrorMessage);
        }
        private static ValidationResult IsCpfcnpj(string cpfcnpj, string errorMessage)
        {
            var d = new int[14];
            var v = new int[2];
            int j, i, soma;
            var soNumero = Regex.Replace(cpfcnpj, "[^0-9]", string.Empty);
            if (new string(soNumero[0], soNumero.Length) == soNumero)
            {
                return new ValidationResult(errorMessage);
            }
            switch (soNumero.Length)
            {
                case 11:
                    {
                        for (i = 0; i <= 10; i++) d[i] = Convert.ToInt32(soNumero.Substring(i, 1));
                        for (i = 0; i <= 1; i++)
                        {
                            soma = 0;
                            for (j = 0; j <= 8 + i; j++) soma += d[j] * (10 + i - j);

                            v[i] = (soma * 10) % 11;
                            if (v[i] == 10) v[i] = 0;
                        }
                        return (v[0] == d[9] & v[1] == d[10]) ? null : new ValidationResult(errorMessage);
                    }
                case 14:
                    {
                        const string sequencia = "6543298765432";
                        for (i = 0; i <= 13; i++) d[i] = Convert.ToInt32(soNumero.Substring(i, 1));
                        for (i = 0; i <= 1; i++)
                        {
                            soma = 0;
                            for (j = 0; j <= 11 + i; j++)
                                soma += d[j] * Convert.ToInt32(sequencia.Substring(j + 1 - i, 1));

                            v[i] = (soma * 10) % 11;
                            if (v[i] == 10) v[i] = 0;
                        }
                        return (v[0] == d[12] & v[1] == d[13]) ? null : new ValidationResult(errorMessage);
                    }
                default:
                    return new ValidationResult(errorMessage);
            }
        }
    }
}

