using System;
using System.Collections.Generic;
using System.Text;

namespace Smart_Report
{
    public class OcrRegressionCase
    {
        public string Name;
        public string Raw;
        public string Expected;
    }

    public class OcrRegressionTests
    {
        public string Run(OcrBlockBuilder builder)
        {
            List<OcrRegressionCase> tests = CreateTests();
            StringBuilder result = new StringBuilder();
            int passed = 0;

            for (int i = 0; i < tests.Count; i++)
            {
                string actual = Normalize(builder.BuildBracketText(tests[i].Raw));
                string expected = Normalize(tests[i].Expected);
                bool ok = actual == expected;

                if (ok) passed++;

                result.Append(ok ? "PASS  " : "FAIL  ");
                result.Append(tests[i].Name);
                result.Append(Environment.NewLine);

                if (!ok)
                {
                    result.Append("Expected:");
                    result.Append(Environment.NewLine);
                    result.Append(tests[i].Expected);
                    result.Append(Environment.NewLine);
                    result.Append("Actual:");
                    result.Append(Environment.NewLine);
                    result.Append(builder.BuildBracketText(tests[i].Raw));
                    result.Append(Environment.NewLine);
                }

                result.Append(Environment.NewLine);
            }

            int analyzerPassed = RunAnalyzerRegressionTests(builder, result);
            int total = tests.Count + 2;
            int totalPassed = passed + analyzerPassed;

            result.Insert(0,
                "Tests: " + total +
                "   Passed: " + totalPassed +
                "   Failed: " + (total - totalPassed) +
                Environment.NewLine + Environment.NewLine);

            return result.ToString();
        }

        private int RunAnalyzerRegressionTests(OcrBlockBuilder builder, StringBuilder result)
        {
            int passed = 0;
            OcrContentAnalyzer analyzer = new OcrContentAnalyzer();

            string fullRaw = "DR. HEMATI CLINIC ID 57530 Name Zolghadri, Soheila Exam Date 2026-10-04 OB Fetal Biometry m1 m2 m3 GA GP Lt FL 66.74 mm BPD 84.58 mm";
            string fullResult = analyzer.Analyze(builder.Build(fullRaw));

            bool fullOk =
                fullResult.Contains("Section=FetalBiometry | Field=SectionMarker | Text=Fetal Biometry") &&
                fullResult.Contains("Section=FetalBiometry | Field=TableHeader | Text=m1 m2 m3 GA GP") &&
                !fullResult.Contains("Text=etry ");

            AppendAnalyzerResult(result, "Analyzer full Fetal Biometry marker + TableHeader", fullOk, fullResult);
            if (fullOk) passed++;

            string fallbackRaw = "Name Test ID 1 Exam. Date 2025-06-01 [OB] Fetal Biom... Last 1 2 3 GA Pctl. BPD 3.96 cm";
            string fallbackResult = analyzer.Analyze(builder.Build(fallbackRaw));

            bool fallbackOk =
                fallbackResult.Contains("Section=FetalBiometry | Field=SectionMarker | Text=Fetal Biometry") &&
                fallbackResult.Contains("Section=FetalBiometry | Field=TableHeader | Text=Last 1 2 3 GA Pctl.") &&
                fallbackResult.Contains("Section=FetalBiometry | Field=BPD");

            AppendAnalyzerResult(result, "Analyzer truncated Fetal Biom fallback", fallbackOk, fallbackResult);
            if (fallbackOk) passed++;

            return passed;
        }

        private void AppendAnalyzerResult(StringBuilder result, string name, bool ok, string actual)
        {
            result.Append(ok ? "PASS  " : "FAIL  ");
            result.Append(name);
            result.Append(Environment.NewLine);

            if (!ok)
            {
                result.Append("Actual Analyzer Output:");
                result.Append(Environment.NewLine);
                result.Append(actual);
                result.Append(Environment.NewLine);
            }

            result.Append(Environment.NewLine);
        }

        private string Normalize(string value)
        {
            return value.Replace("\r\n", "\n").Trim();
        }

        private OcrRegressionCase Case(string name, string raw, string expected)
        {
            OcrRegressionCase c = new OcrRegressionCase();
            c.Name = name; c.Raw = raw; c.Expected = expected;
            return c;
        }

        private List<OcrRegressionCase> CreateTests()
        {
            List<OcrRegressionCase> t = new List<OcrRegressionCase>();

            t.Add(Case("Samsung BPD + HC image",
                "SAMSUNG 57402(1999-04-14) DR. HEMATI CLINIC 2026-09-28 Javadi, Leyla 17:56:35 3rd Trimester/ CA1-7S / 16.0cm/ 26Hz TIs 0.1 / TIb 0.1 / MI 1.0 [2D] SAMSUNG ce Gen Gn 40 DR 116 FA 2 P 90% AR. 5 10 1 BPD 45.91 mm GA 19w6d±12d EDD 2027-02-16 2 HC 168.40 mm GA 19w3d±11d EDD 2027-02-19",
                "[SAMSUNG 57402(1999-04-14) DR. HEMATI CLINIC 2026-09-28 Javadi, Leyla 17:56:35 3rd Trimester/ CA1-7S / 16.0cm/ 26Hz TIs 0.1 / TIb 0.1 / MI 1.0 [2D] SAMSUNG ce Gen Gn 40 DR 116 FA 2 P 90% AR. 5 10 1]\n[BPD 45.91 mm GA 19w6d±12d EDD 2027-02-16 2]\n[HC 168.40 mm GA 19w3d±11d EDD 2027-02-19]"));

            t.Add(Case("Samsung CRL image",
                "SAMSUNG 57398 (1995-03-21) DR. HEMATI CLINIC 2026-09-28 Noori, Mahpare 17:10:00 NT/CA1-7S/14.0cm/32Hz Tls 0.3/ TIb 0.3/MI 1.0 [2D] SAMSUNG Res Gn 41 DR 90 FA 3 P 90% 5 CRL 62.13 mm GA 12w4d±7d EDD 2027-04-08",
                "[SAMSUNG 57398 (1995-03-21) DR. HEMATI CLINIC 2026-09-28 Noori, Mahpare 17:10:00 NT/CA1-7S/14.0cm/32Hz Tls 0.3/ TIb 0.3/MI 1.0 [2D] SAMSUNG Res Gn 41 DR 90 FA 3 P 90% 5]\n[CRL 62.13 mm GA 12w4d±7d EDD 2027-04-08]"));

            t.Add(Case("Samsung NT conditional measurement",
                "SAMSUNG 57398(1995-03-21) DR. HEMATI CLINIC 2026-09-28 Noori, Mahpare 17:12:22 NT/CA1-7S/14.0cm/32Hz TIs 0.3 / TIb 0.3 / MI 1.0 [2D] SAMSUNG JS Res Gn 41 DR 90 FA 3 5 P 90% 十.+ 1 NT 2.32 mm",
                "[SAMSUNG 57398(1995-03-21) DR. HEMATI CLINIC 2026-09-28 Noori, Mahpare 17:12:22 NT/CA1-7S/14.0cm/32Hz TIs 0.3 / TIb 0.3 / MI 1.0 [2D] SAMSUNG JS Res Gn 41 DR 90 FA 3 5 P 90% 十.+ 1]\n[NT 2.32 mm]"));

            t.Add(Case("Samsung NB conditional measurement",
                "SAMSUNG 57398(1995-03-21) DR. HEMATI CLINIC 2026-09-28 Noori, Mahpare 17:08:37 NT/CA1-7S/13.0cm/34Hz TIs 0.2 / TIb 0.2 / MI 1.0 [2D] SAMSUNG SS Res Gn 41 DR 90 FA 3 P 90% NB 2.66 mm",
                "[SAMSUNG 57398(1995-03-21) DR. HEMATI CLINIC 2026-09-28 Noori, Mahpare 17:08:37 NT/CA1-7S/13.0cm/34Hz TIs 0.2 / TIb 0.2 / MI 1.0 [2D] SAMSUNG SS Res Gn 41 DR 90 FA 3 P 90%]\n[NB 2.66 mm]"));

            t.Add(Case("Samsung generic D measurement",
                "SAMSUNG 57402(1999-04-14) DR. HEMATI CLINIC 2026-09-28 Javadi, Leyla 18:00:05 Fetal Heart / CA1-7S / 12.0cm/65Hz TIs 0.2 / TIb 0.2 / MI 1.0 SAMSUNG [2D] V8 Gen Gn 35 DR 106 FA 2 P 90% 七中 5 D 2.78 mm",
                "[SAMSUNG 57402(1999-04-14) DR. HEMATI CLINIC 2026-09-28 Javadi, Leyla 18:00:05 Fetal Heart / CA1-7S / 12.0cm/65Hz TIs 0.2 / TIb 0.2 / MI 1.0 SAMSUNG [2D] V8 Gen Gn 35 DR 106 FA 2 P 90% 七中 5]\n[D 2.78 mm]"));

            t.Add(Case("Samsung indexed D measurement attached ordinal",
                "SAMSUNG Renal [2D] Gen Gn 50 DR 90 P 90% 1D 5.77 mm",
                "[SAMSUNG Renal [2D] Gen Gn 50 DR 90 P 90% 1]\n[D 5.77 mm]"));

            t.Add(Case("Samsung indexed D measurement upper one-digit boundary",
                "SAMSUNG Renal [2D] 9D 4.31 mm",
                "[SAMSUNG Renal [2D] 9]\n[D 4.31 mm]"));

            t.Add(Case("Indexed D guards preserve non-measurement contexts",
                "SAMSUNG [2D] 2D Calculations 2D Measurements SVD 6.4cm 2GD 21.91 mm 1{2D137.04 mm 12D 5.77 mm 1D5.77 mm",
                "[SAMSUNG [2D] 2D Calculations 2D Measurements SVD 6.4cm 2GD 21.91 mm 1{2D137.04 mm 12D 5.77 mm 1D5.77 mm]"));

            t.Add(Case("Samsung LVOT remains unsplit",
                "SAMSUNG 57402 (1999-04-14) DR. HEMATI CLINIC 2026-09-28 Javadi, Leyla 18:01:35 1st FetalHeart / CA1-7S / 12.0cm / 62Hz TIs 0.2/ TIb 0.2 /MI 1.0 SAMSUNG [2D] Gen Gn 33 DR 100 FA 4 D 90% 5 LVOT",
                "[SAMSUNG 57402 (1999-04-14) DR. HEMATI CLINIC 2026-09-28 Javadi, Leyla 18:01:35 1st FetalHeart / CA1-7S / 12.0cm / 62Hz TIs 0.2/ TIb 0.2 /MI 1.0 SAMSUNG [2D] Gen Gn 33 DR 100 FA 4 D 90% 5 LVOT]"));

            t.Add(Case("Samsung RVOT remains unsplit",
                "SAMSUNG 57402(1999-04-14) DR.HEMATICLINIC 2026-09-28 Javadi, Leyla 18:08:47 1st FetalHeart / CA1-7S/12.0cm/62Hz s Tls 0.2/ TIb 0.2/MI 1.0 [2D] SAMSUNG V8 Gen Gn 24 DR 100 FA 4 P 90% 5 RVOT",
                "[SAMSUNG 57402(1999-04-14) DR.HEMATICLINIC 2026-09-28 Javadi, Leyla 18:08:47 1st FetalHeart / CA1-7S/12.0cm/62Hz s Tls 0.2/ TIb 0.2/MI 1.0 [2D] SAMSUNG V8 Gen Gn 24 DR 100 FA 4 P 90% 5 RVOT]"));

            t.Add(Case("Samsung Doppler settings remain unsplit",
                "SAMSUNG 57402 (1999-04-14) DR. HEMATI CLINIC 2026-09-28 Javadi, Leyla 18:07:27 1st FetalHeart / CA1-7S / 12.0cmS8 Tls 0.4/TIb1.7/MI 0.36 SAMSUNG [2D] Gen Gn 37 DR 100 FA 4 P 92% [PW] Gen Gn 50 PRF 4.55kHz 15 WF 104Hz P 90% SV 4.0mm A 0° SVD 6.3cm MV -50 cm/s 50",
                "[SAMSUNG 57402 (1999-04-14) DR. HEMATI CLINIC 2026-09-28 Javadi, Leyla 18:07:27 1st FetalHeart / CA1-7S / 12.0cmS8 Tls 0.4/TIb1.7/MI 0.36 SAMSUNG [2D] Gen Gn 37 DR 100 FA 4 P 92% [PW] Gen Gn 50 PRF 4.55kHz 15 WF 104Hz P 90% SV 4.0mm A 0° SVD 6.3cm MV -50 cm/s 50]"));

            t.Add(Case("Samsung single OB report",
                "DR. HEMATI CLINIC ID 57402 Name Javadi, Leyla Date of Birth(Age) 1999-04-14(27y5m) Gender F Exam Date 2026-09-28 Indication Diag. Physician Ref. Physician Mahak Papen Operator OB LMP GA(LMP) EDD(LMP) Gravida Para Composite GA Average GA(AUA) 19w3d EDD(AUA) 2027-02-19 Ectopic Aborta DOC Ovulation Date EFW1 Hadlock2 BPD,AC,FL 282 g ±42 g (10oz) 19w1d Hadlock EFW2 Hadlock2 BPD,AC,FL 282 g ±42 g (10oz) 19w1d Hadlock Fetal Biometry m1 m2 m3 GA GP Lt FL 29.28 29.28 mm Last 19w0d (17w1d~20w6d) Hadlock Hadlock BPD 45.91 45.91 mm Last 19w6d (18w1d~21w4d) Hadlock Hadlock AC 138.45 138.45 mm Last 19w2d (17w1d~21w2d) Hadlock Hadlock HC 168.40 168.40 mm Last 19w3d (18w0d~21w0d) Hadlock Hadlock 2D Calculations HC/AC 1.22 (~) Campbell",
                "[DR. HEMATI CLINIC ID 57402 Name Javadi, Leyla Date of Birth(Age) 1999-04-14(27y5m) Gender F Exam Date 2026-09-28 Indication Diag. Physician Ref. Physician Mahak Papen Operator]\n[OB]\n[LMP]\n[GA(LMP)]\n[EDD(LMP)]\n[Gravida]\n[Para]\n[Composite GA Average]\n[GA(AUA) 19w3d]\n[EDD(AUA) 2027-02-19]\n[Ectopic]\n[Aborta]\n[DOC]\n[Ovulation Date]\n[EFW1 Hadlock2 BPD,AC,FL 282 g ±42 g (10oz) 19w1d Hadlock]\n[EFW2 Hadlock2 BPD,AC,FL 282 g ±42 g (10oz) 19w1d Hadlock]\n[Fetal Biometry m1 m2 m3 GA GP]\n[Lt FL 29.28 29.28 mm Last 19w0d (17w1d~20w6d) Hadlock Hadlock]\n[BPD 45.91 45.91 mm Last 19w6d (18w1d~21w4d) Hadlock Hadlock]\n[AC 138.45 138.45 mm Last 19w2d (17w1d~21w2d) Hadlock Hadlock]\n[HC 168.40 168.40 mm Last 19w3d (18w0d~21w0d) Hadlock Hadlock]\n[2D Calculations]\n[HC/AC 1.22 (~) Campbell]"));

            t.Add(Case("EFW dot separators stay together",
                "EFW2 Hadlock2 BPD.AC.FL 493 g ±74 g (1Ib 1oz) 22w1d Hadlock Fetal Biometry",
                "[EFW2 Hadlock2 BPD.AC.FL 493 g ±74 g (1Ib 1oz) 22w1d Hadlock]\n[Fetal Biometry]"));

            t.Add(Case("Right FL label",
                "Fetal Biometry m1 m2 m3 GA GP Rt FL 37.71 37.71 mm Last 22w0d",
                "[Fetal Biometry m1 m2 m3 GA GP]\n[Rt FL 37.71 37.71 mm Last 22w0d]"));

            t.Add(Case("GE OFD parentheses and ratios",
                "BPD 81.32mm GA 32w5d OFD (HC) 97.37mm HC 291.71mm GA 32w1d CI (BPD/OFD) 83% AC 279.28mm HC/AC 1.04 FL 61.77mm EFW 1897g FL/AC 22% FL/BPD 76% FL/HC 0.21",
                "[BPD 81.32mm GA 32w5d]\n[OFD (HC) 97.37mm]\n[HC 291.71mm GA 32w1d]\n[CI (BPD/OFD) 83%]\n[AC 279.28mm]\n[HC/AC 1.04]\n[FL 61.77mm]\n[EFW 1897g]\n[FL/AC 22%]\n[FL/BPD 76%]\n[FL/HC 0.21]"));

            t.Add(Case("Twin hierarchical sections",
                "OB (Fetus A) LMP GA(LMP) EDD(LMP) Gravida Para Composite GA Average GA(AUA) 22w1d EDD(AUA) 2027-01-31 EFW1 Hadlock2 BPD,AC,FL 493 g Fetal Biometry m1 m2 m3 GA GP Rt FL 37.71 mm BPD 52.10 mm AC 174.20 mm HC 194.10 mm 2D Calculations HC/AC 1.11 OB (Fetus B) LMP GA(LMP) EDD(LMP) Gravida Para Composite GA Average GA(AUA) 21w6d EDD(AUA) 2027-02-02 EFW2 Hadlock2 BPD.AC.FL 470 g Fetal Biometry Lt FL 36.80 mm BPD 51.20 mm AC 170.00 mm HC 191.00 mm 2D Calculations HC/AC 1.12 OB (Fetus Compare) Composite GA A B Estimated Fetal Weight A B Fetal Biometry A B 2D Calculations A B",
                "[OB (Fetus A)]\n[LMP]\n[GA(LMP)]\n[EDD(LMP)]\n[Gravida]\n[Para]\n[Composite GA Average]\n[GA(AUA) 22w1d]\n[EDD(AUA) 2027-01-31]\n[EFW1 Hadlock2 BPD,AC,FL 493 g]\n[Fetal Biometry m1 m2 m3 GA GP]\n[Rt FL 37.71 mm]\n[BPD 52.10 mm]\n[AC 174.20 mm]\n[HC 194.10 mm]\n[2D Calculations]\n[HC/AC 1.11]\n[OB (Fetus B)]\n[LMP]\n[GA(LMP)]\n[EDD(LMP)]\n[Gravida]\n[Para]\n[Composite GA Average]\n[GA(AUA) 21w6d]\n[EDD(AUA) 2027-02-02]\n[EFW2 Hadlock2 BPD.AC.FL 470 g]\n[Fetal Biometry]\n[Lt FL 36.80 mm]\n[BPD 51.20 mm]\n[AC 170.00 mm]\n[HC 191.00 mm]\n[2D Calculations]\n[HC/AC 1.12]\n[OB (Fetus Compare)]\n[Composite GA A B]\n[Estimated Fetal Weight A B]\n[Fetal Biometry A B]\n[2D Calculations A B]"));

            t.Add(Case("Empty OB parameters are preserved",
                "OB LMP GA(Clin) EDD(GA) Gravida Para DOC Ovulation Date Composite GA Average GA(AUA) 12w4d EDD(AUA) 2027-04-08",
                "[OB]\n[LMP]\n[GA(Clin)]\n[EDD(GA)]\n[Gravida]\n[Para]\n[DOC]\n[Ovulation Date]\n[Composite GA Average]\n[GA(AUA) 12w4d]\n[EDD(AUA) 2027-04-08]"));

            t.Add(Case("GE preset OB must not split",
                "BPD 81.32mm 3 Trim./OB GA 32w5d EDD 2026-11-18 OFD (HC) 97.37mm HC 291.71mm",
                "[BPD 81.32mm 3 Trim./OB GA 32w5d EDD 2026-11-18]\n[OFD (HC) 97.37mm]\n[HC 291.71mm]"));

            t.Add(Case("Samsung measurement with interleaved settings",
                "SAMSUNG [2D] Gen Gn47 BPD 79.28 mm Gen GA 31w6d Gn47 HC 288.76 mm DR116 GA 31w5d FA2 LtFL 61.12 mm GA 31w5d",
                "[SAMSUNG [2D] Gen Gn47]\n[BPD 79.28 mm Gen GA 31w6d Gn47]\n[HC 288.76 mm DR116 GA 31w5d FA2]\n[LtFL 61.12 mm GA 31w5d]"));

            t.Add(Case("Repeated OCR chunks are preserved",
                "OB LMP GA(LMP) Fetal Biometry BPD 50.00 mm HC 180.00 mm OB LMP GA(LMP) Fetal Biometry BPD 50.00 mm HC 180.00 mm",
                "[OB]\n[LMP]\n[GA(LMP)]\n[Fetal Biometry]\n[BPD 50.00 mm]\n[HC 180.00 mm]\n[OB]\n[LMP]\n[GA(LMP)]\n[Fetal Biometry]\n[BPD 50.00 mm]\n[HC 180.00 mm]"));

            t.Add(Case("Composite GA and Estimated Fetal Weight sections",
                "OB (Fetus Compare) Composite GA A 22w1d B 21w6d Estimated Fetal Weight A 493 g B 470 g Fetal Biometry A B",
                "[OB (Fetus Compare)]\n[Composite GA A 22w1d B 21w6d]\n[Estimated Fetal Weight A 493 g B 470 g]\n[Fetal Biometry A B]"));

            t.Add(Case("Samsung CRL with trailing device settings",
                "SAMSUNG 73202(1994-01-31) DRTAGHIZADEH 2026-07-06 Gaderi, Neda 20:59:19 Uterus/EA2-11AR/6.0cm/37Hz Tls 0.4 / TIb 0.4 / MI 1.2 [2D] CRL 4.31 mm Gen GA 6w1d±4d 2027-02-28 Gn 45 DR 102 SAMSUNG FA 3 V8 P 90% 18 1 22 4- XX 13 144 5 十 16",
                "[SAMSUNG 73202(1994-01-31) DRTAGHIZADEH 2026-07-06 Gaderi, Neda 20:59:19 Uterus/EA2-11AR/6.0cm/37Hz Tls 0.4 / TIb 0.4 / MI 1.2 [2D]]\n[CRL 4.31 mm Gen GA 6w1d±4d 2027-02-28 Gn 45 DR 102 SAMSUNG FA 3 V8 P 90% 18 1 22 4- XX 13 144 5 十 16]"));

            t.Add(Case("Samsung D with OCR dot separator",
                "SAMSUNG 73202(1994-01-31) DR.TAGHIZADEH 2026-07-06 Gaderi, Neda 20:58:54 Uterus/EA2-11AR /6.0cm /37Hz TIs 0.4/ TIb 0.4 / MI 1.2 [2D] D.4.10 mm Gen Gn 45 DR 102 SAMSUNG FA 3 V8 p 90% 18 11 12 4- 十 3 13 4 5 16",
                "[SAMSUNG 73202(1994-01-31) DR.TAGHIZADEH 2026-07-06 Gaderi, Neda 20:58:54 Uterus/EA2-11AR /6.0cm /37Hz TIs 0.4/ TIb 0.4 / MI 1.2 [2D]]\n[D.4.10 mm Gen Gn 45 DR 102 SAMSUNG FA 3 V8 p 90% 18 11 12 4- 十 3 13 4 5 16]"));

            t.Add(Case("Samsung TAI quantitative index remains intact",
                "SAMSUNG 73181(1982-03-21) DR.TAGHIZADEH 2026-07-06 Soleymani, Farshid 19:47:16 Abdomen/CA1-7S/16.0cm / 42Hz TIs 0.3 / TIb 0.3 / MI 1.1 [2D] Pen 2.0 Gn 52 DR 90 SAMSUNG SAMSUNG J FA 5 ōP 90% 10 0.0 70 1 15 115 TAI [dB/cm/MHz] 0.98 (R² 0.69)",
                "[SAMSUNG 73181(1982-03-21) DR.TAGHIZADEH 2026-07-06 Soleymani, Farshid 19:47:16 Abdomen/CA1-7S/16.0cm / 42Hz TIs 0.3 / TIb 0.3 / MI 1.1 [2D] Pen 2.0 Gn 52 DR 90 SAMSUNG SAMSUNG J FA 5 ōP 90% 10 0.0 70 1 15 115 TAI [dB/cm/MHz] 0.98 (R² 0.69)]"));

            t.Add(Case("Samsung TSI quantitative index remains intact",
                "SAMSUNG 73181(1982-03-21) DR.TAGHIZADEH 2026-07-06 Soleymani, Farshid 19:47:19 Abdomen /CA1-7S/16.0cm/42Hz TIs 0.3 / TIb 0.3 / MI 1.1 [2D] Pen 200 Gn 52 DR 90 SAMSUNG SAMSUNG UC FA 5 ōP 90% 70 70 15 TSI 110.01",
                "[SAMSUNG 73181(1982-03-21) DR.TAGHIZADEH 2026-07-06 Soleymani, Farshid 19:47:19 Abdomen /CA1-7S/16.0cm/42Hz TIs 0.3 / TIb 0.3 / MI 1.1 [2D] Pen 200 Gn 52 DR 90 SAMSUNG SAMSUNG UC FA 5 ōP 90% 70 70 15 TSI 110.01]"));

            t.Add(Case("Malformed multi-D OCR preserves raw boundaries",
                "SAMSUNG 73178(1979-03-21) DR.TAGHI ZADEH 2026-07-06 Faridian, Rana 21:06:33 Uterus /CA1-7S/14.0cm/32Hz TIs 0.1/ TIb 0.1/MI 1.4 1{2D137.04 mm 3 D 37.34 mm 2GD 21.91 mm D 21.77 mm Gn 27 DR 104 SAMSUNG SAMSUNG V8 18 FA 4 P C 90% 10 X 十 -+ 十 次 u T0 10",
                "[SAMSUNG 73178(1979-03-21) DR.TAGHI ZADEH 2026-07-06 Faridian, Rana 21:06:33 Uterus /CA1-7S/14.0cm/32Hz TIs 0.1/ TIb 0.1/MI 1.4 1{2D137.04 mm 3]\n[D 37.34 mm 2GD 21.91 mm]\n[D 21.77 mm Gn 27 DR 104 SAMSUNG SAMSUNG V8 18 FA 4 P C 90% 10 X 十 -+ 十 次 u T0 10]"));

            t.Add(Case("Generic HR must not start a block",
                "SAMSUNG 73178(1979-03-21) DR.TAGHIZADEH 2026-07-06 Faridian, Rana 21:06:53 Uterus/CA1-7S/14.0cm/32Hz HR TIs 0.1/ TIb 0.1/ MI 1.4 [2D] D 112.82 mm Gen D 50.98 mm Gn 27 SAMSING DR 104 VB FA 4 P 90% 10 15 10",
                "[SAMSUNG 73178(1979-03-21) DR.TAGHIZADEH 2026-07-06 Faridian, Rana 21:06:53 Uterus/CA1-7S/14.0cm/32Hz HR TIs 0.1/ TIb 0.1/ MI 1.4 [2D]]\n[D 112.82 mm Gen]\n[D 50.98 mm Gn 27 SAMSING DR 104 VB FA 4 P 90% 10 15 10]"));

            t.Add(Case("Fetal HR measurement",
                "SAMSUNG 57398 (1995-03-21) DR HEMATLCUINIC 2026-09-28 Noori, Mahpare 17:13:41 NT/CA1-7S/14.0cmS TIs 0.4 / TIb 1.7 / MI 0.56 [2D] SAMSUNG vs Res Gn 59 DR 90 FA 3 P 92% [PW] 15 Gen PRF 3.57kHz Gn 50 WF 61Hz P 90% SV 2.0mm A 0° SVD 6.4cm 10 -50 -40 -30 二20 10 cm/s -10 -20 -30 40 Fetal HR158-bpm",
                "[SAMSUNG 57398 (1995-03-21) DR HEMATLCUINIC 2026-09-28 Noori, Mahpare 17:13:41 NT/CA1-7S/14.0cmS TIs 0.4 / TIb 1.7 / MI 0.56 [2D] SAMSUNG vs Res Gn 59 DR 90 FA 3 P 92% [PW] 15 Gen PRF 3.57kHz Gn 50 WF 61Hz P 90% SV 2.0mm A 0° SVD 6.4cm 10 -50 -40 -30 二20 10 cm/s -10 -20 -30 40]\n[Fetal HR158-bpm]"));

            t.Add(Case("OCR report with truncated sections long bones and compact ratio",
                "H Name Hoseyni Por, Marziye... ID 116271 Exam. Date 2025-06-01 Gender Female BirthDate 1984-03-21 Age 41yr 2m Institute RAD Med.Image ins. Ref. Physician Arezo Shahmoradi Description 2852 T [OB] AUA 17w5d EDD(AUA) 2025-11-04 PctI. Crit... EDD(GA) EFW 214g±32g (8oz) EFW Aut... Hadlock3(AC... GA(EFW) 17w5d Fetal Biom... Last 1 2 3 GA Pctl. BPD 3.96 3.96 cm 18w0d±12d Hadl... HC 15.14 15.14 cm 18w1d±10d Hadl... AC 12.25 12.25 cm 17w6d±11d Hadl... FL 2.59 2.59 cm 17w6d±9d Hadl... Fetal Long... Last 1 2 3 GA Pctl. Lt. HUM 2.37 2.37 cm 17w2d±19d Jeanty ULNA 2.16 2.16 cm 17w3d±22d Jeanty Rt. TIB 2.18 2.18 cm 17w5d±20d Jeanty Fetal HR Last 1 2 3 Fetal HR 143 143 bpm Ratio Value Normal Range 5,255FL/AC 21.16 % (20.0~24.0%, >21w) -1. 2025-06-0117:36",
                "[H Name Hoseyni Por, Marziye... ID 116271 Exam. Date 2025-06-01 Gender Female BirthDate 1984-03-21 Age 41yr 2m Institute RAD Med.Image ins. Ref. Physician Arezo Shahmoradi Description 2852 T]\n[[OB]]\n[AUA 17w5d]\n[EDD(AUA) 2025-11-04 PctI. Crit...]\n[EDD(GA)]\n[EFW 214g±32g (8oz)]\n[EFW Aut... Hadlock3(AC... GA(EFW) 17w5d]\n[Fetal Biom... Last 1 2 3 GA Pctl.]\n[BPD 3.96 3.96 cm 18w0d±12d Hadl...]\n[HC 15.14 15.14 cm 18w1d±10d Hadl...]\n[AC 12.25 12.25 cm 17w6d±11d Hadl...]\n[FL 2.59 2.59 cm 17w6d±9d Hadl...]\n[Fetal Long... Last 1 2 3 GA Pctl.]\n[Lt. HUM 2.37 2.37 cm 17w2d±19d Jeanty]\n[ULNA 2.16 2.16 cm 17w3d±22d Jeanty]\n[Rt. TIB 2.18 2.18 cm 17w5d±20d Jeanty Fetal HR Last 1 2 3]\n[Fetal HR 143 143 bpm Ratio Value Normal Range 5,255]\n[FL/AC 21.16 % (20.0~24.0%, >21w) -1. 2025-06-0117:36]"));

            t.Add(Case("Samsung conditional HR bpm measurement",
                "SAMSUNG 74309(1988-03-21) DR.TAGHI ZADEH 2026-07-08 soleymanzade, rota 16:44:28 3rd Trimester / CA1-7S / 16.0cm Tls 0.5/TIb 1.9/MI 0.54 [2D] SAMSUNG Gen 10 Gn 47 DR 116 HR 147 bpm FA 2 Time 408 ms P 92% [PW] Gen [u Gn 50 PRF 3.57kHz WF 61Hz P 90% SV 2.0mm A 0° 10 SVD 7.6cm 5 -50 -40 -30 -20 -10 cm/s --10 --20 -30 -40 -50",
                "[SAMSUNG 74309(1988-03-21) DR.TAGHI ZADEH 2026-07-08 soleymanzade, rota 16:44:28 3rd Trimester / CA1-7S / 16.0cm Tls 0.5/TIb 1.9/MI 0.54 [2D] SAMSUNG Gen 10 Gn 47 DR 116]\n[HR 147 bpm FA 2 Time 408 ms P 92% [PW] Gen [u Gn 50 PRF 3.57kHz WF 61Hz P 90% SV 2.0mm A 0° 10 SVD 7.6cm 5 -50 -40 -30 -20 -10 cm/s --10 --20 -30 -40 -50]"));

            t.Add(Case("Samsung Fetaļ HR OCR glyph variant",
                "SAMSUNG 57398(1995-03-21) DR. HEMATI CLINIC 2026-09-28 Noori, Mahpare 17:13:41 NT/CA1-7S/14.0cm TIs 0.4 / TIb 1.7 / MI 0.56 [2D] SAMSUNG 79 Res Gn 59 DR 1 90 FA 3 P 92% [PW] 5 Gen Gn 50 PRF 3.57kHz WF 61Hz P 90% SV 2.0mm A 0° SVD 6.4cm 10 -50 -40 -30 -20 10 cm/s -10 -20 -30 40 1 Fetaļ HR158-bpm",
                "[SAMSUNG 57398(1995-03-21) DR. HEMATI CLINIC 2026-09-28 Noori, Mahpare 17:13:41 NT/CA1-7S/14.0cm TIs 0.4 / TIb 1.7 / MI 0.56 [2D] SAMSUNG 79 Res Gn 59 DR 1 90 FA 3 P 92% [PW] 5 Gen Gn 50 PRF 3.57kHz WF 61Hz P 90% SV 2.0mm A 0° SVD 6.4cm 10 -50 -40 -30 -20 10 cm/s -10 -20 -30 40 1]\n[Fetaļ HR158-bpm]"));

            t.Add(Case("Samsung Feta HR dropped-l OCR variant",
                "SAMSUNG 57398(1995-03-21) DR. HEMATI CLINIC 2026-09-28 Noori, Mahpare 17:13:41 NT/CA1-7S/14.0cm TIs 0.4 / TIb 1.7 / MI 0.56 [2D] SAMSUNG 79 Res Gn 59 DR 1 90 FA 3 P 92% [PW] 5 Gen Gn 50 PRF 3.57kHz WF 61Hz P 90% SV 2.0mm A 0° SVD 6.4cm 10 -50 -40 -30 -20 10 cm/s -10 -20 -30 40 1 Feta HR158-bpm",
                "[SAMSUNG 57398(1995-03-21) DR. HEMATI CLINIC 2026-09-28 Noori, Mahpare 17:13:41 NT/CA1-7S/14.0cm TIs 0.4 / TIb 1.7 / MI 0.56 [2D] SAMSUNG 79 Res Gn 59 DR 1 90 FA 3 P 92% [PW] 5 Gen Gn 50 PRF 3.57kHz WF 61Hz P 90% SV 2.0mm A 0° SVD 6.4cm 10 -50 -40 -30 -20 10 cm/s -10 -20 -30 40 1]\n[Feta HR158-bpm]"));

            t.Add(Case("Feta HR without numeric bpm must not split",
                "SAMSUNG Feta HR TIs 0.1 / TIb 0.1 / MI 1.4",
                "[SAMSUNG Feta HR TIs 0.1 / TIb 0.1 / MI 1.4]"));

            // این Test از Sample واقعی Ratio Table آمده است.
            // دلیل: OCR پرانتز Normal Range مربوط به FL/HC را ناقص خوانده و ')' وجود ندارد.
            // با وجود این، HC/AC یک Ratio مستقل است و باید Block جدید بسازد.
            t.Add(Case("Ratio after truncated normal range",
                "Name Hoseyni Por, Marziye... ID 116271 Exam. Date 2025-06-01 Gender Female BirthDate 1984-03-21 Age 41yr 2m Institute RAD Med.Image ins. Ref. Physician Arezo Shahmoradi Description 2852 T Ratio Value Normal Range FL/BPD 65.56 % (71.0~87.0%, >23w) FL/HC 17.13 % (13.30~23.90%, 15... HC/AC 1.24 (0.87~1.39, 13–42w)",
                "[Name Hoseyni Por, Marziye... ID 116271 Exam. Date 2025-06-01 Gender Female BirthDate 1984-03-21 Age 41yr 2m Institute RAD Med.Image ins. Ref. Physician Arezo Shahmoradi Description 2852 T Ratio Value Normal Range]\n[FL/BPD 65.56 % (71.0~87.0%, >23w)]\n[FL/HC 17.13 % (13.30~23.90%, 15...]\n[HC/AC 1.24 (0.87~1.39, 13–42w)]"));

            return t;
        }
    }
}
