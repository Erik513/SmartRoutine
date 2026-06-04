using SmartRoutine.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace SmartRoutine.Logic.Services
{
    public static class AutoRunBuilder
    {
        public static List<RoutineStep> CreateAutoRunSteps(Routine routine)
        {
            if (routine == null || routine.Steps == null)
                return new List<RoutineStep>();

            return routine.Steps
                .Where(s => s.Show)
                .OrderBy(s => s.Order)
                .Select(CreateAutoRunStepCopy)
                .Where(s => s != null)
                .ToList();
        }

        public static RoutineStep CreateAutoRunStepCopy(RoutineStep step)
        {
            return CreateStepCopy(step, true);
        }

        public static RoutineStep CreateNormalStepCopy(RoutineStep step)
        {
            return CreateStepCopy(step, false);
        }

        private static RoutineStep CreateStepCopy(
                    RoutineStep step,
                    bool forceAutoRun)
        {
            if (step == null)
                return null;

            if (step is OpenUrlStep)
            {
                OpenUrlStep urlStep = (OpenUrlStep)step;

                return new OpenUrlStep
                {
                    Id = urlStep.Id,
                    Order = urlStep.Order,
                    Name = urlStep.Name,
                    Description = urlStep.Description,
                    Show = urlStep.Show,
                    AutoStart = forceAutoRun ? true : urlStep.AutoStart,
                    Url = urlStep.Url,
                    OpenInExternalBrowser = forceAutoRun
                        ? true
                        : urlStep.OpenInExternalBrowser
                };
            }

            if (step is OpenFolderStep)
            {
                OpenFolderStep folderStep = (OpenFolderStep)step;

                return new OpenFolderStep
                {
                    Id = folderStep.Id,
                    Order = folderStep.Order,
                    Name = folderStep.Name,
                    Description = folderStep.Description,
                    Show = folderStep.Show,
                    AutoStart = forceAutoRun ? true : folderStep.AutoStart,
                    FolderPath = folderStep.FolderPath,
                    OpenInNewWindow = folderStep.OpenInNewWindow
                };
            }

            if (step is OpenApplicationStep)
            {
                OpenApplicationStep appStep = (OpenApplicationStep)step;

                return new OpenApplicationStep
                {
                    Id = appStep.Id,
                    Order = appStep.Order,
                    Name = appStep.Name,
                    Description = appStep.Description,
                    Show = appStep.Show,
                    AutoStart = forceAutoRun ? true : appStep.AutoStart,
                    ApplicationPath = appStep.ApplicationPath,
                    Arguments = appStep.Arguments,
                    RunAsAdmin = appStep.RunAsAdmin,
                    WorkingDirectory = appStep.WorkingDirectory
                };
            }

            if (step is OpenDocumentStep)
            {
                OpenDocumentStep docStep = (OpenDocumentStep)step;

                return new OpenDocumentStep
                {
                    Id = docStep.Id,
                    Order = docStep.Order,
                    Name = docStep.Name,
                    Description = docStep.Description,
                    Show = docStep.Show,
                    AutoStart = forceAutoRun ? true : docStep.AutoStart,
                    FilePath = docStep.FilePath,
                    OpenWithAssociatedApp = docStep.OpenWithAssociatedApp
                };
            }

            return null;
        }
    }
}