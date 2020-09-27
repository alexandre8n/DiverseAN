using System;
using System.Collections.Generic;
using System.Text;

namespace MoleculeGenTest
{
    public class TransformStep
    {
        public string startingElement;
        public List<string> tempTarget = new List<string>();
        public List<string> moleculaElements = new List<string>();
        public List<TransformRule> moleculeTransforms = new List<TransformRule>();

        public int CurrentPosition => LengthOfTargetReached;
        public string CurrentTarget => tempTarget[LengthOfTargetReached];
        public string CurrentElmToTransfer => moleculaElements[LengthOfTargetReached];
        public int LengthOfTargetReached = 0;
        public bool TargetIsReached = false;

        public string VerboseTarget()
        {
            string s1 = $"Target: {string.Join("", tempTarget)} Pos={CurrentPosition}";
            return s1;
        }

        public string Verbose()
        {
            string s1 = $"Mol: {CurrentResultingMolecula}, Pos={CurrentPosition}";
            return s1;
        }
        public string CurrentResultingMolecula => string.Join("", moleculaElements);

        public TransformStep(string startingElement, List<string> tempTarget)
        {
            this.startingElement = startingElement;
            this.tempTarget.AddRange(tempTarget);
            moleculaElements.Add(startingElement);
        }

        internal void AddTransform(TransformRule trn)
        {
            moleculeTransforms.Add(trn);
        }

        internal void ProceedWithStep(List<string> currentMolecula, List<int> indicesOfTransformsDone, int currentPosition)
        {
            // take an element in cur pos
            // replace it by transforms to do
        }

        internal bool IsGoodMatchForTarget()
        {
            return (LengthOfTargetReached >= tempTarget.Count);

        }

        // returns number of added elements
        internal int BuildMolecula(TransformRule usedTransform)
        {
            string elementToTrn = moleculaElements[CurrentPosition];
            var replacingElms = usedTransform.toElements;
            moleculaElements.RemoveAt(CurrentPosition);
            moleculaElements.InsertRange(CurrentPosition, replacingElms);
            int cntOfEqualInTarget = CountTrnMatchesWithTarget(moleculaElements, CurrentPosition);
            if(cntOfEqualInTarget > 0)
            {
                LengthOfTargetReached += cntOfEqualInTarget;
            }
            return cntOfEqualInTarget;
        }
        private int CountTrnMatchesWithTarget(List<string> chemElementsInMolecula, int curPos)
        {
            int cnt = 0;
            int commonMin = Math.Min(chemElementsInMolecula.Count, tempTarget.Count);
            for (int i = curPos; i < commonMin; i++)
            {
                if (chemElementsInMolecula[i] == tempTarget[i])
                {
                    cnt++;
                }
                else
                    return cnt;
            }
            return 0;
        }

        internal TransformStep Copy()
        {
            TransformStep aCopy = new TransformStep(CurrentElmToTransfer, tempTarget);
            aCopy.tempTarget.Clear();
            aCopy.tempTarget.AddRange(tempTarget);
            aCopy.moleculaElements.Clear();
            aCopy.moleculaElements.AddRange(moleculaElements);
            aCopy.moleculeTransforms.Clear(); 
            aCopy.moleculeTransforms.AddRange(moleculeTransforms);
            aCopy.LengthOfTargetReached = LengthOfTargetReached;
            aCopy.TargetIsReached = TargetIsReached;
            return aCopy;
    }

        internal void RestoreFrom(TransformStep trnStepSaved)
        {
            startingElement = trnStepSaved.startingElement;
            tempTarget.Clear();
            tempTarget.AddRange(trnStepSaved.tempTarget);
            moleculaElements.Clear();
            moleculaElements.AddRange(trnStepSaved.moleculaElements);
            moleculeTransforms.Clear();
            moleculeTransforms.AddRange(trnStepSaved.moleculeTransforms);
            LengthOfTargetReached = trnStepSaved.LengthOfTargetReached;
            TargetIsReached = trnStepSaved.TargetIsReached;
        }
    }
}
