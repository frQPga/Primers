using Primer;

PrimerCandidate[] candidates = new PrimerCandidate[20000];
PrimerCandidate[] reverseCandidates = new PrimerCandidate[20000];

int incrementingId = 0;
double targetTm = 60.0;
int minimumLength = 18;
int maximumLength = 24;
int[] GCContentRange = new int[] { 30, 80 };
int HairpinMin = 4;
int SelfDimerMin = 5;

System.Console.WriteLine("Type file path for input DNA sequence:");
string filePath = System.Console.ReadLine();
System.Console.WriteLine("Set target melting temperature (Tm):");
targetTm = double.Parse(System.Console.ReadLine());
System.Console.WriteLine("Set minimum primer length:");
minimumLength = int.Parse(System.Console.ReadLine());
System.Console.WriteLine("Set maximum primer length:");
maximumLength = int.Parse(System.Console.ReadLine());
System.Console.WriteLine("Set GC content range (e.g., 30-80):");
string[] gcRange = System.Console.ReadLine().Split('-');
GCContentRange[0] = int.Parse(gcRange[0]);
GCContentRange[1] = int.Parse(gcRange[1]);

string dnaSequence = System.IO.File.ReadAllText(filePath).ToUpper().Replace("\n", "").Replace("\r", "").Replace(" ", "").Replace("\t", "").Replace(",", "");
for (int length = minimumLength; length <= maximumLength; length++)
{
    for (int i = 0; i <= dnaSequence.Length - length; i++)
    {
        string candidateSeq = dnaSequence.Substring(i, length);
        PrimerCandidate candidate = new PrimerCandidate(incrementingId++, candidateSeq);
        candidate.Reverse = false;
        candidate.startingPosition = i;
        candidates[candidate.Id] = candidate;
    }
}

System.Console.WriteLine("Forward primers:");
foreach (var candidate in candidates)
{
    if (candidate != null)
    {
        qualifyPrimer(candidate, targetTm, GCContentRange, HairpinMin, SelfDimerMin);
    }
    if (candidate != null && candidate.Qualifies)
    {
        System.Console.WriteLine($"Primer ID: {candidate.Id}, Position: {candidate.startingPosition}, Sequence: {candidate.Sequence}, Tm: {candidate.CalculateTm():F2}°C, GC Content: {candidate.CalculateGCContent():F2}%");
    }
}

incrementingId = 0; // Reset ID for reverse primers
string reverseComplement = new string(dnaSequence.Reverse().Select(nucleotide =>
{
    return nucleotide switch
    {
        'A' => 'T',
        'T' => 'A',
        'G' => 'C',
        'C' => 'G',
        _ => nucleotide
    };
}).ToArray());
for (int length = minimumLength; length <= maximumLength; length++)
{
    for (int i = 0; i <= reverseComplement.Length - length; i++)
    {
        string candidateSeq = reverseComplement.Substring(i, length);
        PrimerCandidate candidate = new PrimerCandidate(incrementingId++, candidateSeq);
        candidate.Reverse = true;
        candidate.startingPosition = i;
        reverseCandidates[candidate.Id] = candidate;
    }
}
System.Console.WriteLine("Reverse primers:");
foreach (var candidate in reverseCandidates)
{
    if (candidate != null)
    {
        qualifyPrimer(candidate, targetTm, GCContentRange, HairpinMin, SelfDimerMin);
    }
    if (candidate != null && candidate.Qualifies)
    {
        System.Console.WriteLine($"Primer ID: {candidate.Id}, Position: {candidate.startingPosition}, Sequence: {candidate.Sequence}, Tm: {candidate.CalculateTm():F2}°C, GC Content: {candidate.CalculateGCContent():F2}%");
    }
}

static void qualifyPrimer(PrimerCandidate candidate, double targetTm, int[] GCContentRange, int HairpinMin, int SelfDimerMin)
{
    if (candidate.IsValid() && 
        Math.Abs(candidate.CalculateTm() - targetTm) <= 0.5 &&
        candidate.CalculateGCContent() >= GCContentRange[0] && 
        candidate.CalculateGCContent() <= GCContentRange[1] &&
        candidate.MaxRepeats() <= 3 &&
        candidate.HairpinScore(HairpinMin) == 0 &&
        candidate.SelfDimerScore(SelfDimerMin) == 0 &&
        candidate.ThreePrimeGCCount() <= 2
    )
    {
        candidate.Qualifies = true;
    }
    else
    {
        candidate.Qualifies = false;
    }
}