using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Channels;
namespace Primer
{
    public class PrimerCandidate
    {
        public int Id { get; set; }
        public string Sequence { get; set; }
        public int startingPosition { get; set; }

        public bool Reverse { get; set; }

        public PrimerCandidate(int id, string sequence)
        {
            Id = id;
            Sequence = sequence;
        }
        public double CalculateTm()
        {
            int atCount = Sequence.Count(nucleotide => nucleotide == 'A' || nucleotide == 'T');
            int gcCount = Sequence.Count(nucleotide => nucleotide == 'G' || nucleotide == 'C');
            return 100.5 + 41 * gcCount / (double)Sequence.Length - 820 / (double)Sequence.Length + 16.6*Math.Log10(0.05);
        }
        public double CalculateGCContent()
        {
            int gcCount = Sequence.Count(nucleotide => nucleotide == 'G' || nucleotide == 'C');
            return (double)gcCount / Sequence.Length * 100;
        }

        public bool IsValid()
        {
            return Sequence.All(nucleotide => "ATGC".Contains(nucleotide));
        }

        public int ThreePrimeGCCount()
        {
            int count = 0;
        if (Reverse == false) {
            for (int i = Sequence.Length - 1; i >= Sequence.Length - 5 && i >= 0; i--)
            {
                if (Sequence[i] == 'G' || Sequence[i] == 'C')
                {
                   count++;
                }
            }
        } else {
            for (int i = 0; i < 5 && i < Sequence.Length; i++)
            {
                if (Sequence[i] == 'G' || Sequence[i] == 'C')
                {
                   count++;
                }
            }
        }
            return count;
        }

        public bool Qualifies { get; set; }

        public int SelfDimerScore(int minSize)
        {
            int score = 0;
            for (int i = 0; i < Sequence.Length - minSize; i++)
            {
                for (int j = i + minSize; j < Sequence.Length - minSize; j++)
                {
                    if (SeqIsComplementary(Sequence.Substring(i, minSize), Sequence.Substring(j, minSize)))
                    {
                        score++;
                    }
                }
            }
            return score;
        }

        public int HairpinScore (int minSize)
        {
            int score = 0;
            for (int i = 0; i < Sequence.Length - minSize; i++)
            {
                for (int j = Sequence.Length - minSize; j > 0; j--)
                {
                    if (SeqIsComplementary(Sequence.Substring(i, minSize), reverse(Sequence.Substring(j, minSize))))
                    {
                        score++;
                    }
                }
            }
            return score;
        }

        private bool SeqIsComplementary(string a, string b)
        {
            int marginofError = 1;
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (!IsComplementary(a[i], b[i]))
                {
                    marginofError--;
                    if (marginofError < 0) return false;
                }
            }
            return true;
        }

        private string reverse(string a)
        {
            char[] charArray = a.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        private bool IsComplementary(char a, char b)
        {
            return (a == 'A' && b == 'T') || (a == 'T' && b == 'A') ||
                   (a == 'G' && b == 'C') || (a == 'C' && b == 'G');
        }


        public int MaxRepeats()
        {
            int maxRepeats = 1;
            int currentRepeats = 1;
            for (int i = 1; i < Sequence.Length; i++)
            {
                if (Sequence[i] == Sequence[i - 1])
                {
                    currentRepeats++;
                    maxRepeats = Math.Max(maxRepeats, currentRepeats);
                }
                else
                {
                    currentRepeats = 1;
                }
            }
            return maxRepeats;
        }
    }
}