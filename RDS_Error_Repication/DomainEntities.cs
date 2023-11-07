using System.Net.Mail;
using System.Text.RegularExpressions;

namespace RDS_Error_Repication
{
    public abstract class ValueObject
    {
        protected static bool EqualOperator(ValueObject left, ValueObject right)
        {
            if (left is null ^ right is null)
            {
                return false;
            }

            return left?.Equals(right!) != false;
        }

        protected static bool NotEqualOperator(ValueObject left, ValueObject right)
        {
            return !(EqualOperator(left, right));
        }

        protected abstract IEnumerable<object> GetEqualityComponents();

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var other = (ValueObject)obj;
            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Select(x => x != null ? x.GetHashCode() : 0)
                .Aggregate((x, y) => x ^ y);
        }
    }


    public class CellNumber : ValueObject
    {
        public string _Cell { get; private set; }
        public static string DEFAULT_CELL_NUMBER = "0000000000";

        private CellNumber()
        {
        }
        private CellNumber(string Cell)
        {
            _Cell = Cell;
        }

        public static bool CheckCellNumberValidity(string Cell)
        {
            Cell = Cell.Trim();
            return Regex.Match(Cell, @"^(0[0-9]{9})$").Success;
        }

        public static CellNumber From(string Cell)
        {
            if (!CheckCellNumberValidity(Cell))
            {
                throw new Exception(Cell);
            }
            return new CellNumber(Cell);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return _Cell;
        }
        public override string ToString()
        {
            return _Cell;
        }
    }
    public class EmailAddress : ValueObject
    {
        public string _Email { get; private set; }


        private EmailAddress()
        {

        }
        private EmailAddress(string Email)
        {
            _Email = Email.ToLower();
        }
        public static bool CheckEmailValidity(string Email)
        {

            if (Email.Trim().EndsWith("."))
            {
                return false;
            }
            try
            {
                var mailAddress = new MailAddress(Email);
                return mailAddress.Address == Email;
            }
            catch
            {
                return false;
            }
        }
        public static EmailAddress From(string Email)
        {
            if (!CheckEmailValidity(Email))
            {
                throw new Exception(Email);
            }
            return new EmailAddress(Email);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return _Email;
        }
        public override string ToString()
        {
            return _Email;
        }
    }
}