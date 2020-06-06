namespace Warhammer.Core.Entities
{
    public partial class Term
    {
        public string AgeBracket
        {
            get
            {
                int startAge = 14;
                for (int i = 0; i <= TermNumber; i++)
                {
                    startAge = startAge + i;
                }

                int endAge = startAge + TermNumber + 1;

                if (startAge == 14)
                {
                    return "0 - 14";
                }
                else
                {
                    return $"{startAge} - {endAge}";
                }
            }
        }
    }
}