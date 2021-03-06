﻿namespace entity.Brillo.Document
{
    public static class Start
    {
        public static void Manual(object Document, app_document_range Range)
        {
            if (Range != null)
            {
                Normal Normal = new Normal(Document, Range, Normal.PrintStyles.Manual);
            }
        }

        public static void Automatic(object Document, app_document_range Range)
        {
            if (Range != null)
            {
                Normal Normal = new Normal(Document, Range, Normal.PrintStyles.Automatic);
            }
        }

        public static void Automatic(object Document, string DocumentName)
        {
            if (DocumentName != "")
            {
                Normal Normal = new Normal(Document, DocumentName);
            }
        }

        public static void Automatic(object Document, string DocumentName,app_location app_Location)
        {
            if (DocumentName != "")
            {
                Normal Normal = new Normal(Document, DocumentName, app_Location);
            }
        }
    }
}