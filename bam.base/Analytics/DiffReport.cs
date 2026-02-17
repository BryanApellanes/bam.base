/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Analytics
{

    /// <summary>
    /// Represents the result of comparing two text strings, containing tokens for unchanged, inserted, and deleted lines.
    /// </summary>
    [Serializable]
    public class DiffReport : IDiffReport
    {
        Dictionary<string, Action<string>> _savers;
        /// <summary>
        /// Initializes a new instance of the <see cref="DiffReport"/> class with empty token collections and default save support for JSON.
        /// </summary>
        public DiffReport()
        {
            tokens = new List<DiffReportToken>();
            inserted = new List<InsertedDiffReportToken>();
            deleted = new List<DeletedDiffReportToken>();
            Dictionary<string, Action<string>> savers = new Dictionary<string, Action<string>>
            {
                { ".json", this.ToJsonFile }
            };
            this._savers = savers;
            
        }

        List<DiffReportToken> tokens;
        /// <summary>
        /// Gets or sets all diff tokens (unchanged, inserted, and deleted lines) in order.
        /// </summary>
        public DiffReportToken[] Tokens
        {
            get => tokens.ToArray();
            set
            {
                tokens.Clear();
                tokens.AddRange(value);
            }
        }

        List<InsertedDiffReportToken> inserted;
        /// <summary>
        /// Gets the tokens representing lines that were inserted in the modified text.
        /// </summary>
        public InsertedDiffReportToken[] Inserted => inserted.ToArray();

        List<DeletedDiffReportToken> deleted;
        /// <summary>
        /// Gets the tokens representing lines that were deleted from the original text.
        /// </summary>
        public DeletedDiffReportToken[] Deleted => deleted.ToArray();

        /// <summary>
        /// Saves this report to the specified file, overwriting it if it already exists.
        /// Uses the file extension to determine the format; defaults to JSON.
        /// </summary>
        /// <param name="filePath">The file path to save the report to.</param>
        public void Save(string filePath)
        {
            string ext = System.IO.Path.GetExtension(filePath);
            if (_savers.ContainsKey(ext))
            {
                _savers[ext](filePath);
            }
            else
            {
                this.ToJsonFile(filePath);
            }
        }

        private void AddLine<T>(int lineNum, string text) where T : DiffReportToken, new()
        {
            T line = new T();
            line.lineNum = lineNum;
            line.text = text;
            tokens.Add(line);
            if (line.Type == DiffType.Deleted)
            {
                deleted.Add((line as DeletedDiffReportToken)!);
            }
            if (line.Type == DiffType.Inserted)
            {
                inserted.Add((line as InsertedDiffReportToken)!);
            }

        }
                
        /// <summary>
        /// Creates a diff report comparing two strings line by line, using newline as the separator.
        /// </summary>
        /// <param name="a">The original text.</param>
        /// <param name="b">The modified text.</param>
        /// <returns>A <see cref="DiffReport"/> containing tokens for unchanged, inserted, and deleted lines.</returns>
        public static DiffReport Create(string a, string b)
        {
            return Create(a, b, '\n');
        }

        /// <summary>
        /// Creates a diff report comparing two strings, splitting on the specified separator characters.
        /// </summary>
        /// <param name="a">The original text.</param>
        /// <param name="b">The modified text.</param>
        /// <param name="separators">Characters used to split the text into comparable segments.</param>
        /// <returns>A <see cref="DiffReport"/> containing tokens for unchanged, inserted, and deleted lines.</returns>
        public static DiffReport Create(string a, string b, params char[] separators)
        {
            DiffReport report = new DiffReport();

            Diff.Item[] f = Diff.DiffText(a, b, true, true, false, separators);
            string[] aLines = a.Split(separators);
            string[] bLines = b.Split(separators);

            int n = 1;
            for (int fdx = 0; fdx < f.Length; fdx++)
            {
                Diff.Item aItem = f[fdx];

                // write unchanged lines
                while ((n-1 < aItem.StartB) && (n-1 < bLines.Length))
                {
                    report.AddLine<DiffReportToken>(n, bLines[n-1]);
                    n++;
                } // while

                // write deleted lines
                for (int m = 0; m < aItem.deletedA; m++)
                {
                    report.AddLine<DeletedDiffReportToken>(-1, aLines[aItem.StartA + m]);
                } // for

                // write inserted lines
                while (n-1 < aItem.StartB + aItem.insertedB)
                {
                    report.AddLine<InsertedDiffReportToken>(n, bLines[n-1]);
                    n++;
                } // while
            } // while

            // write rest of unchanged lines
            while (n-1 < bLines.Length)
            {
                report.AddLine<DiffReportToken>(n, bLines[n-1]);
                n++;
            } // while

            return report;
        }
    }
}
