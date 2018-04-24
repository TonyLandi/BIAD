using System;
using Microsoft.Bot.Builder.FormFlow;

namespace LabBot
{
    public enum DepartmentOptions
    {
        Accounting,
        AdministrativeSupport,
        IT
    }

    [Serializable]
    public class SurveyForm
    {
        [Prompt("Please enter your name {&}.")]
        public string Name;

        [Prompt("Please enter your {&}.")]
        public string PhoneNumber;

        [Prompt("Please enter your {&}.")]
        [Pattern(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}")]
        public string EmailAddress;

        [Prompt("What {&} do you work in? {||}.")]
        public DepartmentOptions? Department;

        public static IForm<SurveyForm> BuildForm()
        {
            return new FormBuilder<SurveyForm>().Build();
        }
    }
}