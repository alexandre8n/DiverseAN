using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MoleculeGenTest
{
    public class ModelInfo
    {
        public string Molecule = "";
        public string StrMoleculeTransforms = "";
        List<string> transformStrings = new List<string>();
        List<string> chemElementsInTargetMolecula = new List<string>();
        List<TransformRule> moleculeTransform = new List<TransformRule>();
        int currentPosition = 0;
        int numberOfTempTargetElements = 25;
        List<int> stepsDone = new List<int>();
        List<string> currentMolecula = new List<string>();
        List<int> indicesOfTransformsDone = new List<int>();
        const int maxLevelHowDeep = 30;
        
        Dictionary<string, Dictionary<string, string>> OneElmTransfPossibleDictionary = new Dictionary<string, Dictionary<string, string>>();

        int maxLengthReached = 0;
        string currentMaxLenMorlecula;
        public ModelInfo() { }
        public bool ReadSourceInfo(string fileName)
        {
            StreamReader file = new StreamReader(fileName);
            string line;
            int index = 0;
            while ((line = file.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(line))
                    continue;
                TransformRule mdlTransf = TransformRule.PrcTransformLine(line, index++);
                if (mdlTransf != null)
                {
                    transformStrings.Add(line);
                    moleculeTransform.Add(mdlTransf);  
                }
                else 
                {
                    Molecule = line;
                    chemElementsInTargetMolecula = TransformRule.GetElementsFromString(line);
                    break;
                }
                
            }
            file.Close();
            BuildOneElmTransfPossibleDictionary();
            return true;
        }

        private void BuildOneElmTransfPossibleDictionary()
        {
            foreach (var trn in moleculeTransform)
            {
                if(!OneElmTransfPossibleDictionary.ContainsKey(trn.fromElm))
                {
                    var toElms = new Dictionary<string, string>();
                    toElms[trn.toElements[0]] = $"{trn.fromElm}->{trn.toElements[0]};";
                    OneElmTransfPossibleDictionary[trn.fromElm] = toElms;
                    continue;
                }
                var toElmDict = OneElmTransfPossibleDictionary[trn.fromElm];
                toElmDict[trn.toElements[0]] = $"{trn.fromElm}->{trn.toElements[0]};";
            }

            List<string> fromElmsInDict = OneElmTransfPossibleDictionary.Keys.ToList();
            foreach (string elm1 in fromElmsInDict)
            {
                Dictionary<string, string> resElms = new Dictionary<string, string>();
                // dict of elm, and sequences a->b; b->c;....
                Dictionary<string, string> elmsFromElm1 = GetPossibleTransf(elm1, ref resElms);
                OneElmTransfPossibleDictionary[elm1] = elmsFromElm1;
            }
            SaveDictToFile();
        }

        private void SaveDictToFile()
        {
            string fileInLog = Utl.logFilePath;
            Utl.logFilePath = Utl.PathOfLogFile + "OneElmTransfPossibleDictionary.txt";
            foreach (var kv in OneElmTransfPossibleDictionary)
            {
                Dictionary<string, string> valDict = kv.Value;
                Utl.Log(kv.Key + "-> ");
                string transfers = "";
                foreach (var kv1 in valDict)
                {
                    transfers = kv1.Key + "->" + kv1.Value;
                    Utl.Log("       " + transfers);
                }
            }
            Utl.logFilePath = fileInLog;
        }

        // dict of elm, and sequences a->b; b->c;....
        private Dictionary<string, string> GetPossibleTransf(string elm1, ref Dictionary<string, string> resElmsInput)
        {
            int countOfInputElms = resElmsInput.Count;
            if (!OneElmTransfPossibleDictionary.ContainsKey(elm1))
            {
                var dict = new Dictionary<string, string>(resElmsInput);
                return dict;
            }
            Dictionary<string, string> current = OneElmTransfPossibleDictionary[elm1];
            var resElms = Merge2Dict(current, resElmsInput);

            if (countOfInputElms == resElms.Count)
            {
                return resElms;
            }

            int countInresElms = resElms.Count;
            while (true)
            {
                Dictionary<string, string> resElms2 = new Dictionary<string, string>();
                resElms2 = Merge2Dict(current, resElms2);
                for (int i=0; i < resElms2.Keys.Count; i++)
                {
                    var elm2 = resElms2.Keys.ToList()[i];
                    Dictionary<string, string> resElms3 = GetPossibleTransf(elm2, ref resElms2);
                    resElms2 = Merge2Dict(resElms2, resElms3);
                }

                if (resElms2.Count > resElms.Count)
                {
                    resElms = Merge2Dict(current, resElms2);
                }
                else
                {
                    break;
                }
            }
            return resElms;
        }

        public static Dictionary<string, string> Merge2Dict(Dictionary<string, string> d1, Dictionary<string, string> d2)
        {
            var merged = d1.Concat(d2)
                .ToLookup(x => x.Key, x => x.Value)
                .ToDictionary(x => x.Key, g => g.First());
            foreach (var kv in d2)
            {
                if (merged.ContainsKey(kv.Key))
                {
                    string valueOfmerged = merged[kv.Key];
                    string d2Val = d2[kv.Key];
                    if (d2Val.IndexOf(valueOfmerged) > -1)
                        merged[kv.Key] = d2[kv.Key];
                    else if (valueOfmerged.IndexOf(d2Val) == -1)
                        merged[kv.Key] += d2[kv.Key];
                }
            }
            return merged;
        }

        public TransformStep ProcessToTargetFrom(string startingElement)
        {
            currentPosition = 0;
            List<string> tempTarget = new List<string>();
            tempTarget.AddRange(chemElementsInTargetMolecula.Take(numberOfTempTargetElements));
            // select best match for first temp target elements
            // it should make matching at least for the 1st element of tempTarget

            var trnStp = new TransformStep("e", tempTarget); //!! to change e should not be here
            var trnStpDone = StriveToTarget2(tempTarget, trnStp);

            if (trnStpDone.IsGoodMatchForTarget())
            {
                currentPosition = trnStpDone.CurrentPosition;
            }

            return trnStpDone;
        }

        private TransformStep StriveToTarget(List<string> tempTarget, TransformStep trnStpDone)
        {
            int levelHowDeep = maxLevelHowDeep;
            
            Utl.Log(trnStpDone.Verbose());
            Utl.Log(trnStpDone.VerboseTarget());

            string curElmToStart = trnStpDone.CurrentElmToTransfer;
            List<TransformRule> transformRules = GetTransformsForElement(curElmToStart);
            foreach (var trnRule in transformRules)
            {
                Utl.Log(trnStpDone.Verbose());
                Utl.Log(trnRule.Verbose());
                TransformStep trnStpReservedCopy = trnStpDone.Copy();
                if (IterateTryingToFind(trnRule, trnStpDone, levelHowDeep) &&
                    trnStpDone.LengthOfTargetReached >= tempTarget.Count)
                {
                    return trnStpDone;
                }
                trnStpDone.RestoreFrom(trnStpReservedCopy);
            }

            return trnStpDone;
        }


        // format of string: from -> to
        string currentAttempt;
        List<string> failedAttempts = new List<string>();

        // format: molecula, pos=N, Elm->ResultingElm
        List<string> allTriedMoleculasBeforeBuild = new List<string>();
        List<string> allJustBuiltMoleculas = new List<string>();
        List<string> successReached = new List<string>();
        string CurrAttmpt(TransformStep trnStpDone)
        { 
            return $"Mol:{trnStpDone.CurrentResultingMolecula}, pos={trnStpDone.CurrentPosition}, {trnStpDone.CurrentElmToTransfer} -> {trnStpDone.tempTarget[trnStpDone.LengthOfTargetReached]}";
        }

        private TransformStep StriveToTarget2(List<string> tempTarget, TransformStep trnStpDone)
        {
            int levelHowDeep = maxLevelHowDeep;

            Utl.Log(trnStpDone.Verbose());
            Utl.Log(trnStpDone.VerboseTarget());

            string curElmToStart = trnStpDone.CurrentElmToTransfer;
            List<TransformRule> transformRules = GetTransformsForElement(curElmToStart);
            TransformStep trnStpReservedCopy = trnStpDone.Copy();
            if (IterateTryingToFind2(transformRules, trnStpDone, levelHowDeep))
            {
                if (trnStpDone.LengthOfTargetReached >= trnStpDone.tempTarget.Count)
                {
                    Utl.Log("All done. Target reached");
                }
            }

            return trnStpDone;
        }
        private bool IterateTryingToFind2(List<TransformRule> transformRules, TransformStep trnStpDone, int levelHowDeep)
        {
            levelHowDeep--;
            if (levelHowDeep < 0)
            {
                Utl.Log("!!! Max deep level {maxLevelHowDeep} is reached:");
                Utl.Log(trnStpDone.Verbose());
                foreach(var rule in transformRules)
                    Utl.Log(rule.Verbose());
                return false;
            }
            TransformStep trnStepSaved = trnStpDone.Copy();

            foreach (var rule in transformRules)
            {
                Utl.Log("rule to try " + rule.Verbose());
                string toInRule = string.Join("", rule.toElements);
                string curMol = trnStpDone.CurrentResultingMolecula;
                if (toInRule == "HF" && curMol == "e")
                { 
                }
                if (toInRule == "CRnFYFYFAr" && curMol == "e")
                {
                }
                if (toInRule == "CaF" && curMol == "CRnFYFYFArF")
                {
                }
                if (toInRule == "PB" && curMol == "CRnCaFYFYFArF")
                {
                }
                if (toInRule == "CaP" && curMol == "CRnPBFYFYFArF")
                {
                }
                if (toInRule == "PB" && curMol == "CRnCaPBFYFYFArF")
                {
                }
                if (toInRule == "CaP" && curMol == "CRnPBPBFYFYFArF")
                {
                }
                if (toInRule == "SiRnFYFAr" && curMol == "CRnCaPBPBFYFYFArF")
                {
                }
                if (toInRule == "CaF" && curMol == "CRnSiRnFYFArPBPBFYFYFArF")
                {
                }
                if (toInRule == "PMg" && curMol == "CRnSiRnCaFYFArPBPBFYFYFArF")
                {
                }
                if (toInRule == "TiMg" && curMol == "CRnSiRnCaPMgYFArPBPBFYFYFArF")
                {
                }
                if (toInRule == "CaF" && curMol == "CRnSiRnCaPTiMgYFArPBPBFYFYFArF")
                {
                }
                if (toInRule == "CaF" && curMol == "CRnSiRnCaPTiMgYCaFArPBPBFYFYFArF")
                {
                }
                if (toInRule == "PB" && curMol == "CRnSiRnCaPTiMgYCaCaFArPBPBFYFYFArF")
                {
                }
                if (toInRule == "TiRnFAr" && curMol == "CRnSiRnCaPTiMgYCaPBFArPBPBFYFYFArF")
                {
                }
                if (!MoveWithRule(rule, trnStpDone))
                {
                    failedAttempts.Add(CurrAttmpt(trnStpDone));
                    Utl.Log("Failed iterations " + trnStpDone.Verbose());
                    Utl.Log(CurrAttmpt(trnStpDone));
                    continue;
                }
                    
                if (trnStpDone.LengthOfTargetReached >= trnStpDone.tempTarget.Count)
                    return true;

                List<TransformRule> trnsfRules = GetTransformsForElement(trnStpDone.CurrentElmToTransfer);
                if (IterateTryingToFind2(trnsfRules, trnStpDone, levelHowDeep))
                {
                    Utl.Log("Ok iterations " + trnStpDone.Verbose());
                    return true;
                }
                trnStpDone.RestoreFrom(trnStepSaved);
            }

            currentAttempt = CurrAttmpt(trnStpDone);
            Utl.Log("Failed also " + currentAttempt);

            Utl.Log("Failed Iterate Trying To Find2, deep level {maxLevelHowDeep}");
            Utl.Log(trnStpDone.Verbose());
            foreach (var rule in transformRules)
                Utl.Log(rule.Verbose());
            trnStpDone.RestoreFrom(trnStepSaved);
            Utl.Log("Returned to older trnStep " + trnStpDone.Verbose());
            return false;
        }
        private bool MoveWithRule(TransformRule moleculeTransformRule, TransformStep trnStpDone)
        {
            if (IsEmptyAttempt(moleculeTransformRule, trnStpDone.tempTarget[trnStpDone.LengthOfTargetReached]))
            {
                Utl.Log("Empty attempt:" + trnStpDone.Verbose());
                Utl.Log("Empty attempt rule:" + moleculeTransformRule.Verbose());
                return false;
            }
            if (IsTriedAttempt(trnStpDone, moleculeTransformRule, trnStpDone.tempTarget[trnStpDone.LengthOfTargetReached]))
            {
                Utl.Log("Already tried attempt:" + trnStpDone.Verbose());
                Utl.Log("Already tried attempt rule:" + moleculeTransformRule.Verbose());
                return false;
            }
            currentAttempt = CurrAttmpt(trnStpDone);
            Utl.Log(currentAttempt);

            int lengthOfTargetReachedSoFar = trnStpDone.LengthOfTargetReached;
            string oldMolecula = trnStpDone.CurrentResultingMolecula;
            MakeATryStep(trnStpDone, moleculeTransformRule);
            Utl.Log("Mol.is generated " + trnStpDone.Verbose());
            Utl.Log(moleculeTransformRule.Verbose());
            if (trnStpDone.LengthOfTargetReached >= lengthOfTargetReachedSoFar)
            {
                AddSuccessStory(oldMolecula, lengthOfTargetReachedSoFar, trnStpDone, moleculeTransformRule);
            }
            return true;
        }

        private bool IterateTryingToFind(TransformRule moleculeTransformRule, TransformStep trnStpDone, int levelHowDeep)
        {
            levelHowDeep--;
            if (levelHowDeep < 0)
            {
                Utl.Log("!!! Max deep level {maxLevelHowDeep} is reached:");
                Utl.Log(trnStpDone.Verbose());
                Utl.Log(moleculeTransformRule.Verbose());
                return false;
            }
            if (IsEmptyAttempt(moleculeTransformRule, trnStpDone.tempTarget[trnStpDone.LengthOfTargetReached]))
            {
                Utl.Log("Empty attempt:" + trnStpDone.Verbose());
                Utl.Log("Empty attempt rule:" + moleculeTransformRule.Verbose());
                return false;
            }
            if (IsTriedAttempt(trnStpDone, moleculeTransformRule, trnStpDone.tempTarget[trnStpDone.LengthOfTargetReached]))
            {
                Utl.Log("Already tried attempt:" + trnStpDone.Verbose());
                Utl.Log("Already tried attempt rule:" + moleculeTransformRule.Verbose());
                return false;
            }
            currentAttempt = CurrAttmpt(trnStpDone);
            Utl.Log(currentAttempt);

            levelHowDeep--;
            if (levelHowDeep < 0)
            {
                Utl.Log("!!! Max deep level {maxLevelHowDeep} is reached:");
                Utl.Log(trnStpDone.Verbose());
                Utl.Log(moleculeTransformRule.Verbose());
                return false;
            }
            int lengthOfTargetReachedSoFar = trnStpDone.LengthOfTargetReached;
            TransformStep trnStepSaved = trnStpDone.Copy();
            string oldMolecula = trnStpDone.CurrentResultingMolecula;
            MakeATryStep(trnStpDone, moleculeTransformRule);
            Utl.Log("Mol.is generated "+trnStpDone.Verbose());
            Utl.Log(moleculeTransformRule.Verbose());
            if (trnStpDone.LengthOfTargetReached >= lengthOfTargetReachedSoFar)
            {
                AddSuccessStory(oldMolecula, lengthOfTargetReachedSoFar, trnStpDone, moleculeTransformRule);
                if(trnStpDone.LengthOfTargetReached >= trnStpDone.tempTarget.Count)
                    return true;
            }
            //!!! till here 
            // if not -> in cur pos take an Elm and find transofrms for it. Iterate them and MakeATryStep
            List<TransformRule> moleculeTransforms2 = GetTransformsForElement(trnStpDone.CurrentElmToTransfer);
            foreach (var trn2 in moleculeTransforms2)
            {
                Utl.Log("rule to try "+trn2.Verbose());
                if (IterateTryingToFind(trn2, trnStpDone, levelHowDeep))
                {
                    Utl.Log("Ok iterations " + trnStpDone.Verbose());
                    return true;
                }
            }
            failedAttempts.Add(CurrAttmpt(trnStpDone));
            Utl.Log("Failed iterations " + trnStpDone.Verbose());
            Utl.Log(CurrAttmpt(trnStpDone));

            if (currentAttempt != CurrAttmpt(trnStpDone))
            {
                failedAttempts.Add(currentAttempt);
                Utl.Log("Failed also " + currentAttempt);
            }
            trnStpDone.RestoreFrom(trnStepSaved);
            Utl.Log("Returned to older trnStep " + trnStpDone.Verbose());
            return false;
        }

        // format: key=molecula, value=previousMolecula, pos=curPos(Elm), Rule: from={fromElm}, to={toElms}
        // Rule: from={fromElm}, to={string.Join("", toElements)}
        Dictionary<string, string> moleculaAndAppliedTrnRules = new Dictionary<string, string>();
        private void AddSuccessStory(string oldMolecula, int lengthOfTargetReachedSoFar, TransformStep trnStp, TransformRule mlTrRl)
        {
            string reached = $"{trnStp.CurrentResultingMolecula} NewLen={trnStp.LengthOfTargetReached} OldLen={lengthOfTargetReachedSoFar}";
            Utl.Log("Good elms added: " + reached);
            successReached.Add(reached);
            if (maxLengthReached < trnStp.LengthOfTargetReached)
            {
                maxLengthReached = trnStp.LengthOfTargetReached;
                currentMaxLenMorlecula = trnStp.CurrentResultingMolecula;
            }
            string prevAndRule = $"{oldMolecula}, pos={lengthOfTargetReachedSoFar}({mlTrRl.fromElm}), {mlTrRl.Verbose()}, lenReached={trnStp.LengthOfTargetReached}";
            if (moleculaAndAppliedTrnRules.ContainsKey(trnStp.CurrentResultingMolecula))
            {
                // !!! very strange! again the same molecula
                Utl.LogToFile("StrangeCases.txt", $"The same molecula generated again: {trnStp.CurrentResultingMolecula}");
                Utl.LogToFile("StrangeCases.txt", $"Old was:{moleculaAndAppliedTrnRules[trnStp.CurrentResultingMolecula]}");
                Utl.LogToFile("StrangeCases.txt", $"Now agn:{prevAndRule}");
                moleculaAndAppliedTrnRules[trnStp.CurrentResultingMolecula] += $" [again] {prevAndRule}";
            }
            else
            {
                moleculaAndAppliedTrnRules[trnStp.CurrentResultingMolecula] = prevAndRule;
            }
            Utl.LogToFile("SuccessStories.txt", trnStp.CurrentResultingMolecula+" from: "+ moleculaAndAppliedTrnRules[trnStp.CurrentResultingMolecula]);
        }

        private bool IsTriedAttempt(TransformStep trnStpDone, TransformRule trnRule, string targetElm)
        {
            //return false;
            int len = trnStpDone.CurrentPosition + 2; 
            // we shall take one more char, because it can metter... what will be after replacement
            string moleculaIncludingCurrent = string.Join("", trnStpDone.moleculaElements.Take(len));
            if (FindInTried(moleculaIncludingCurrent, trnStpDone.CurrentPosition, trnStpDone, trnRule))
                return true;
            return false;
        }

        private bool FindInTried(string moleculaIncludingCurrent, int currentPosition, TransformStep trnStp, TransformRule tr)
        {
            string regExpPattern = $"^{moleculaIncludingCurrent}[a-zA-Z]*, pos={currentPosition},"+ BuildTrnTrgStr(trnStp, tr);
            for (int i = allTriedMoleculasBeforeBuild.Count - 1; i >= 0; i--)
            {
                string line = allTriedMoleculasBeforeBuild[i];
                if (Regex.IsMatch(line, regExpPattern))
                    return true; 
            }
            return false;
        }

        private bool IsEmptyAttempt(TransformRule trn, string targetElm)
        {
            if (trn.fromElm == trn.toElements[0] && trn.fromElm != targetElm)
                return true;

            var dictOfRes = OneElmTransfPossibleDictionary[trn.fromElm];

            if (!dictOfRes.ContainsKey(targetElm))
                return true;

            return false;
        }

        private void MakeATryStep(TransformStep trnStpDone, TransformRule trn)
        {
            string strTry = BuildAtryStr(trnStpDone, trn);
            allTriedMoleculasBeforeBuild.Add(strTry);
            Utl.Log("Before trnsf.try " + trnStpDone.Verbose());
            Utl.Log("rule: " + trn.Verbose());

            List<TransformRule> usedTransforms = new List<TransformRule>();
            int nElmMatchedInTarget = trnStpDone.BuildMolecula(trn);
            Utl.Log("After trnsf.try " + trnStpDone.Verbose());

            allJustBuiltMoleculas.Add(MoleculaAfterBuild(trnStpDone));

        }

        private string MoleculaAfterBuild(TransformStep trnStpDone)
        {
            string str = $"{trnStpDone.CurrentResultingMolecula}, pos={trnStpDone.CurrentPosition}," +
                $" trg={trnStpDone.CurrentTarget} L.Rchd={trnStpDone.LengthOfTargetReached}";
            return str;
        }

        // format: molecula, pos=N, Elm->ResultingElm
        private string BuildAtryStr(TransformStep trnStpDone, TransformRule trn)
        {
            string str = $"{trnStpDone.CurrentResultingMolecula}, pos={trnStpDone.CurrentPosition}," + BuildTrnTrgStr(trnStpDone, trn);
            return str;
        }
        private string BuildTrnTrgStr(TransformStep trnStpDone, TransformRule trn)
        {
            return $"{trn.fromElm}->{String.Join("", trn.toElements)} trg={trnStpDone.CurrentTarget}";
        }

        private TransformRule FindNextMatch(TransformStep trnStp)
        {
            List<TransformRule> moleculeTransforms = GetTransformsForElement(trnStp.startingElement);
            foreach (var trn in moleculeTransforms)
            {
                if (IsGoodMatch(trnStp, trn))
                    return trn;

            }
            return null;
        }

        private bool IsGoodMatch(TransformStep trnStp, TransformRule trn)
        {
            int countMatches = CountTrnMatchesWithTarget(trn, trnStp);
            if (countMatches >= 1)
            {
                trnStp.AddTransform(trn);
                return true;
            }
            string startingElement2 = trn.toElements[0];
            List<TransformRule> transofrmIndices2 = GetTransformsForElement(startingElement2);
            foreach (var trn2 in transofrmIndices2)
            {
                int countMatches2 = CountTrnMatchesWithTarget(trn2, trnStp);
                if (countMatches2 >= 1)
                {
                    trnStp.AddTransform(trn);
                    trnStp.AddTransform(trn2);
                    return true;
                }
                    
            }
            return false;
        }

        private int CountTrnMatchesWithTarget(TransformRule trn, TransformStep trnStp)
        {
            int cnt = 0;
            for (int i = 0; i < trn.toElements.Count; i++)
            {
                if (trn.toElements[i] == trnStp.tempTarget[i])
                {
                    cnt++;
                }
                else
                    return cnt;
            }
            return 0;
        }

        private List<TransformRule> GetTransformsForElement(string startingElement)
        {
            List<TransformRule> trns = moleculeTransform.Where(x => x.fromElm == startingElement).ToList();
            return trns;
        }
    }
}
