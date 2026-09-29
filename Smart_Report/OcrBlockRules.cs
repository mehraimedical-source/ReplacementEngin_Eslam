using System;
using System.Collections.Generic;

namespace Smart_Report
{
    public class OcrBlockRules
    {
        public List<string> Sections;
        public List<string> Parameters;
        public List<string> Measurements;

        public OcrBlockRules()
        {
            Sections = new List<string>();
            Parameters = new List<string>();
            Measurements = new List<string>();
        }

        public static OcrBlockRules CreateDefault()
        {
            OcrBlockRules r = new OcrBlockRules();

            r.Sections.Add("Fetal Biometry");
            r.Sections.Add("2D Measurements");
            r.Sections.Add("2D Calculations");
            r.Sections.Add("Composite GA Average");
            r.Sections.Add("OB");

            r.Parameters.Add("Ovulation Date");
            r.Parameters.Add("GA(AUA)");
            r.Parameters.Add("GA(LMP)");
            r.Parameters.Add("GA(Clin)");
            r.Parameters.Add("EDD(AUA)");
            r.Parameters.Add("EDD(LMP)");
            r.Parameters.Add("EDD(GA)");
            r.Parameters.Add("Gravida");
            r.Parameters.Add("Ectopic");
            r.Parameters.Add("Aborta");
            r.Parameters.Add("Para");
            r.Parameters.Add("DOC");
            r.Parameters.Add("LMP");

            r.Measurements.Add("HC/AC");
            r.Measurements.Add("FL/AC");
            r.Measurements.Add("FL/BPD");
            r.Measurements.Add("FL/HC");
            r.Measurements.Add("EFW1");
            r.Measurements.Add("EFW2");
            r.Measurements.Add("EFW");
            r.Measurements.Add("BPD");
            r.Measurements.Add("OFD");
            r.Measurements.Add("CRL");
            r.Measurements.Add("FHR");
            r.Measurements.Add("LT FL");
            r.Measurements.Add("LTFL");
            r.Measurements.Add("FL");
            r.Measurements.Add("HC*");
            r.Measurements.Add("HC");
            r.Measurements.Add("AC");
            r.Measurements.Add("HR");
            r.Measurements.Add("D1");
            r.Measurements.Add("D2");
            r.Measurements.Add("CI");
            r.Measurements.Add("CL");

            return r;
        }

        public OcrBlockRules Clone()
        {
            OcrBlockRules r = new OcrBlockRules();
            r.Sections.AddRange(Sections);
            r.Parameters.AddRange(Parameters);
            r.Measurements.AddRange(Measurements);
            return r;
        }
    }
}
