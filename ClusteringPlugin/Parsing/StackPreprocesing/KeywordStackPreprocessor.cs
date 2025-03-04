using ClusteringPlugin.StackPreprocessing.FunctionDescriptions;
using ClusteringPlugin.Stacks;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClusteringPlugin.StackPreprocessing
{
    internal class FunctionDescriptionStackPreprocessor : IStackPreprocessor
    {
        readonly IFunctionDescriptionProvider functionDescriptionProvider;
        readonly ITextCleaner textCleaner;

        public FunctionDescriptionStackPreprocessor(IFunctionDescriptionProvider functionDescriptionProvider, ITextCleaner textCleaner)
        {
            this.functionDescriptionProvider = functionDescriptionProvider;
            this.textCleaner = textCleaner;
        }

        public string[] PreprocessStacks(Stack[] stacks)
        {
            HashSet<StackFrame> stackFrameSet = new HashSet<StackFrame>();

            foreach (Stack stack in stacks)
            {
                foreach (StackFrame stackFrame in stack.Frames)
                {
                    stackFrameSet.Add(stackFrame);
                }
            }

            StackFrame[] stackFrames = stackFrameSet.ToArray();
            string[] functionDescriptions = functionDescriptionProvider.GetFunctionDescriptions(stackFrames);
            Dictionary<StackFrame, string> functionDescriptionByStackFrame = new Dictionary<StackFrame, string>();

            for (int i = 0; i < stackFrames.Length; i++)
            {
                functionDescriptionByStackFrame.Add(stackFrames[i], functionDescriptions[i]);
            }

            string[] inputStrings = new string[stacks.Length];

            for (int i = 0; i < stacks.Length; i++)
            {
                Stack stack = stacks[i];
                StringBuilder inputString = new StringBuilder();

                foreach (StackFrame stackFrame in stack.Frames)
                {
                    inputString.Append(functionDescriptionByStackFrame[stackFrame] + " ");
                }

                inputStrings[i] = textCleaner.CleanText(inputString.ToString());
            }

            return inputStrings;
        }
    }
}
