using SmartRoutine.Data.Models;
using System.Collections.Generic;

namespace SmartRoutine.Logic.Services
{
    public static class RoutineCloneService
    {
        public static Routine DeepCopy(Routine original)
        {
            if (original == null)
                return null;

            return new Routine
            {
                Id = original.Id,
                Name = original.Name,
                Order = original.Order,
                CreatedAt = original.CreatedAt,
                UpdatedAt = original.UpdatedAt,
                LastExecutionAt = original.LastExecutionAt,
                Steps = CloneSteps(original.Steps)
            };
        }

        private static List<RoutineStep> CloneSteps(List<RoutineStep> steps)
        {
            var clonedSteps = new List<RoutineStep>();

            if (steps == null)
                return clonedSteps;

            foreach (var step in steps)
            {
                clonedSteps.Add(CloneStep(step));
            }

            return clonedSteps;
        }

        private static RoutineStep CloneStep(RoutineStep step)
        {
            switch (step)
            {
                case OpenUrlStep urlStep:
                    return new OpenUrlStep
                    {
                        Id = urlStep.Id,
                        Order = urlStep.Order,
                        Name = urlStep.Name,
                        Description = urlStep.Description,
                        Show = urlStep.Show,
                        AutoStart = urlStep.AutoStart,
                        Url = urlStep.Url,
                        OpenInExternalBrowser = urlStep.OpenInExternalBrowser
                    };

                case OpenFolderStep folderStep:
                    return new OpenFolderStep
                    {
                        Id = folderStep.Id,
                        Order = folderStep.Order,
                        Name = folderStep.Name,
                        Description = folderStep.Description,
                        Show = folderStep.Show,
                        AutoStart = folderStep.AutoStart,
                        FolderPath = folderStep.FolderPath,
                        OpenInNewWindow = folderStep.OpenInNewWindow
                    };

                case OpenApplicationStep appStep:
                    return new OpenApplicationStep
                    {
                        Id = appStep.Id,
                        Order = appStep.Order,
                        Name = appStep.Name,
                        Description = appStep.Description,
                        Show = appStep.Show,
                        AutoStart = appStep.AutoStart,
                        ApplicationPath = appStep.ApplicationPath,
                        Arguments = appStep.Arguments,
                        RunAsAdmin = appStep.RunAsAdmin,
                        WorkingDirectory = appStep.WorkingDirectory
                    };

                case OpenDocumentStep docStep:
                    return new OpenDocumentStep
                    {
                        Id = docStep.Id,
                        Order = docStep.Order,
                        Name = docStep.Name,
                        Description = docStep.Description,
                        Show = docStep.Show,
                        AutoStart = docStep.AutoStart,
                        FilePath = docStep.FilePath,
                        OpenWithAssociatedApp = docStep.OpenWithAssociatedApp
                    };

                default:
                    return null;
            }
        }
    }
}