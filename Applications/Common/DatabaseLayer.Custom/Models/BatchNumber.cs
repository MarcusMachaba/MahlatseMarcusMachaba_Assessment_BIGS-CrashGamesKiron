using DatabaseLayer.Attributes;
using System.Text;

namespace DatabaseLayer.Models
{
    [TableContract(PrimaryKey = "IdBatchNumber")]
    public class BatchNumber
    {
        [ColumnContract(new string[] { })]
        public int IdBatchNumber { get; set; }

        [ColumnContract(new string[] { }, Queryable = true)]
        public int BatchNumberType { get; set; }

        [ColumnContract(new string[] { }, Length = 255)]
        public string CurrentBatchNumber { get; set; }

        public void IncrementBatchNumber()
        {
            bool flag = true;
            for (int index = this.CurrentBatchNumber.Length - 1; index >= 0 & flag; --index)
                flag = this.IncrementCharacter(index);
        }

        private bool IncrementCharacter(int index)
        {
            char c = this.CurrentBatchNumber[index];
            if (char.IsLetter(c))
            {
                if (c == 'z')
                {
                    this.SetCharacter(index, 'A');
                    return false;
                }
                if (c == 'Z')
                {
                    this.SetCharacter(index, 'a');
                    return true;
                }
                this.SetCharacter(index, (char)((uint)c + 1U));
                return false;
            }
            if (c == '9')
            {
                this.SetCharacter(index, '0');
                return true;
            }
            this.SetCharacter(index, (char)((uint)c + 1U));
            return false;
        }

        private void SetCharacter(int index, char newValue)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (index > 0)
                stringBuilder.Append(this.CurrentBatchNumber.Substring(0, index));
            stringBuilder.Append(newValue);
            if (index + 1 < this.CurrentBatchNumber.Length)
                stringBuilder.Append(this.CurrentBatchNumber.Substring(index + 1, this.CurrentBatchNumber.Length - (index + 1)));
            this.CurrentBatchNumber = stringBuilder.ToString();
        }
    }
}
