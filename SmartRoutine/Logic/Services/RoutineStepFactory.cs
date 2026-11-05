using SmartRoutine.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace SmartRoutine.Logic.Services
{
    public static class RoutineStepFactory
    {
        public static List<RoutineStep> CreateAutoRunSteps(Routine routine)
        {
            if (routine == null || routine.Steps == null)
                return new List<RoutineStep>();

            return routine.Steps
                .Where(s => s.Show)
                .OrderBy(s => s.Order)
                .Select(CreateAutoRunCopy)
                .Where(s => s != null)
                .ToList();
        }

        public static RoutineStep CreateAutoRunCopy(RoutineStep step)
        {
            return CreateStepCopy(step, true);
        }

        public static RoutineStep CreateCopy(RoutineStep step)
        {
            return CreateStepCopy(step, false);
        }

        // Copies a step with all its fields. For an auto run the step starts and continues by itself
        // (and a URL opens in the external browser).
        private static RoutineStep CreateStepCopy(
                    RoutineStep step,
                    bool forceAutoRun)
        {
            RoutineStep copy;

            switch (step)
            {
                case null:
                    return null;

                case OpenUrlStep urlStep:
                    copy = new OpenUrlStep
                    {
                        Url = urlStep.Url,
                        OpenInExternalBrowser = forceAutoRun || urlStep.OpenInExternalBrowser
                    };
                    break;

                case OpenFolderStep folderStep:
                    copy = new OpenFolderStep
                    {
                        FolderPath = folderStep.FolderPath,
                        OpenInNewWindow = folderStep.OpenInNewWindow
                    };
                    break;

                case OpenApplicationStep appStep:
                    copy = new OpenApplicationStep
                    {
                        ApplicationPath = appStep.ApplicationPath,
                        Arguments = appStep.Arguments,
                        RunAsAdmin = appStep.RunAsAdmin,
                        WorkingDirectory = appStep.WorkingDirectory
                    };
                    break;

                case OpenDocumentStep docStep:
                    copy = new OpenDocumentStep
                    {
                        FilePath = docStep.FilePath,
                        OpenWithAssociatedApp = docStep.OpenWithAssociatedApp
                    };
                    break;

                default:
                    return null;
            }

            copy.Id = step.Id;
            copy.Order = step.Order;
            copy.Name = step.Name;
            copy.Description = step.Description;
            copy.Show = step.Show;
            copy.AutoStart = forceAutoRun || step.AutoStart;
            copy.AutoContinue = forceAutoRun || step.AutoContinue;

            return copy;
        }
    }
}
