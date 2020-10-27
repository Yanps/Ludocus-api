using AutoWrapper.Wrappers;
using LudocusApi.Controllers;
using LudocusApi.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LudocusApi.Helpers
{
    public class Calculator
    {
        #region Properties
        private Experience main_experience { get; set; }

        private Metric reference_metric { get; set; }

        private MetricController metric_controller { get; set; }

        private MetricValuesController metric_values_controller { get; set; }

        private RuleController rule_controller { get; set; }

        private AchievmentController achievment_controller { get; set; }

        private ExperienceSetController experience_set_controller { get; set; }

        private List<AnalyzableExperienceSet> analyzable_verification_experiences_sets_list { get; set; }

        private List<MetricValues> complex_metrics_values_list { get; set; }

        private List<UserResponse> users_list { get; set; }
        #endregion

        #region Methods
        #region Evaluate Expression
        // Evaluates Expressions with "float" data type
        private bool EvaluateExpression(float metric_value, string rule_operator_code, float rule_value)
        {
            switch (rule_operator_code)
            {
                case "==":
                    return metric_value == rule_value;
                case "!=":
                    return metric_value != rule_value;
                case ">":
                    return metric_value > rule_value;
                case ">=":
                    return metric_value >= rule_value;
                case "<":
                    return metric_value < rule_value;
                case "<=":
                    return metric_value <= rule_value;
                default:
                    return false;
            }
        }

        // Evaluates Expressions with "bool" data type
        private bool EvaluateExpression(bool metric_value, string rule_operator_code, bool rule_value)
        {
            switch (rule_operator_code)
            {
                case "==":
                    return metric_value == rule_value;
                case "!=":
                    return metric_value != rule_value;
                default:
                    return false;
            }
        }

        // Evaluates Expressions with "string" data type and model "a"
        private bool EvaluateExpression(string metric_value, string rule_operator_code, string rule_value)
        {
            switch (rule_operator_code)
            {
                case "==":
                    return metric_value == rule_value;
                case "!=":
                    return metric_value != rule_value;
                default:
                    return false;
            }
        }

        // Evaluates Expressions with "string" data type and model "l"
        private bool EvaluateExpression(List<string> metric_values, string rule_operator_code, string rule_value)
        {
            switch (rule_operator_code)
            {
                case "==":
                    return metric_values.Contains(rule_value);
                case "!=":
                    return !metric_values.Contains(rule_value);
                default:
                    return false;
            }
        }
        #endregion

        #region Affect Metric Values
        private MetricValues AffectMetricValues(MetricValues affected_metric_values, Metric affected_metric, string achievment_effect_value)
        {
            // Checks affected Metric model
            if (affected_metric.model == "l")
            {
                // If affected Metric model is "de lançamento",
                // then adds Achievment's value to the Metric Values' values list
                affected_metric_values.values.Add(achievment_effect_value);
            }
            else if (affected_metric.model == "a")
            {
                // If it's "absoluta", then the action depends on the affected Metric data type
                if (affected_metric.data_type == "float")
                {
                    // If affected Metric data type is "float",
                    // then instantiates new value
                    string old_value = affected_metric_values.values.FirstOrDefault();
                    float new_value = old_value == null ? 0 : float.Parse(old_value, CultureInfo.InvariantCulture);

                    // Adds Achievment value to the Metric Values' average
                    int metric_values_values_count = affected_metric_values.values.Count();
                    if (metric_values_values_count != 0)
                    {
                        // Gets average value from metric_values.values
                        // by Sum Map parsed string value
                        new_value = affected_metric_values.values.Sum(h =>
                        {
                            return float.Parse(h, CultureInfo.InvariantCulture);
                        }) / metric_values_values_count;
                    }

                    // Sums achievment value
                    // When the Achievment have other Operators, will implement it here
                    new_value += float.Parse(achievment_effect_value, CultureInfo.InvariantCulture);

                    // Sets the new value to the Metric Values
                    List<string> new_value_list = new List<string>();
                    new_value_list.Add(new_value.ToString());
                    affected_metric_values.values = new_value_list;
                }
                else if (affected_metric.data_type == "string")
                {

                    // If the main Experience is of type "level",
                    // then the affected Metric values only changes to a higher level title,
                    // and the order is defined on main Experience's level_ordering field
                    if (main_experience.type != "level" ||
                        main_experience.level_ordering
                            .FindIndex(a => a == affected_metric_values.values[0])
                        .CompareTo(main_experience.level_ordering.FindIndex(b => b == achievment_effect_value))
                        < 0) 
                    {
                        // If achievment_effect_value index on level_ordering is bigger than
                        // actual affected Metric values, then the title doesn't change

                        // Or if affected Metric values has no values yet

                        // Or if affected Metric data type is "string"
                        // and Experience type is not "level",
                        // then replaces actual Metric Value with Achievment's value
                        affected_metric_values.values = new List<string>();
                        affected_metric_values.values.Add(achievment_effect_value);
                    }
                    // Else, does nothing, the affected Metric values doesn't change
                }
                else if (affected_metric.data_type == "bool")
                {
                    // If affected Metric data type is "bool",
                    // then replaces actual Metric Value with Achievment's value
                    affected_metric_values.values = new List<string>();
                    affected_metric_values.values.Add(achievment_effect_value);
                }
            }
            // Returns the updated MetricValues
            return affected_metric_values;
        }
        #endregion

        #region Analyze Experience Set by User
        // Analyzes the ExperienceSet over its Metric and Rule and
        // returns Achievment.affected_metric_value if Analysis is positive, null if negative
        private string AnalyzeExperienceSetByUser(AnalyzableExperienceSet analyzable_experience_set, UserResponse user)
        {
            bool evaluation = false;

            // Checks if Analyzable Experience Set's Metric is complex
            // If it is, this Metric Values might be already calculated in complex_metrics_values_list
            if (analyzable_experience_set.metric.classification == "c")
            {
                // Subtitutes the only Metric Values on complex metrics values list
                // which has the same Metric uid and User uid as the analyzable Experience Set
                analyzable_experience_set.metric_values = this.complex_metrics_values_list
                    .Single(mv => mv.metric_uid == analyzable_experience_set.metric.uid &&
                    mv.user_uid == user.uid);
            }

            // Analyze Experience Set by model and data type
            if (analyzable_experience_set.metric.model == "a" && analyzable_experience_set.metric.data_type == "float")
            {
                // If Metric's is of type "Nota única", then compares the float value
                // with rule's operator to perform the expression
                float metric_value = analyzable_experience_set.metric_values.values.FirstOrDefault() == null ? 0 : float.Parse(analyzable_experience_set.metric_values.values.First(), CultureInfo.InvariantCulture);

                // Evaluates the expression
                evaluation = EvaluateExpression(metric_value, analyzable_experience_set.rule.operator_code, float.Parse(analyzable_experience_set.rule.rule_value, CultureInfo.InvariantCulture));
            }
            else if (analyzable_experience_set.metric.model == "l" && analyzable_experience_set.metric.data_type == "float")
            {
                // If Metric is of type "Várias notas",
                // then get's the average of Metric Values and compares it to the Rule value
                float average_metric_value = 0;
                int metric_values_values_count = analyzable_experience_set.metric_values.values.Count();
                if (metric_values_values_count != 0)
                {
                    // Gets average value from metric_values.values
                    // by Sum Map parsed string value
                    average_metric_value = analyzable_experience_set.metric_values.values.Sum(h =>
                    {
                        return float.Parse(h, CultureInfo.InvariantCulture);
                    }) / metric_values_values_count;
                }

                // Evaluates the expression
                evaluation = EvaluateExpression(average_metric_value, analyzable_experience_set.rule.operator_code, float.Parse(analyzable_experience_set.rule.rule_value, CultureInfo.InvariantCulture));
            }
            else if (analyzable_experience_set.metric.model == "a" && analyzable_experience_set.metric.data_type == "bool")
            {
                // If Metric's is of type "Aprovação", then compares the bool value
                // with rule's operator to perform the expression
                // but only if the metric value is not null
                if (analyzable_experience_set.metric_values.values.FirstOrDefault() != null)
                {
                    bool metric_value =  bool.Parse(analyzable_experience_set.metric_values.values.First());
                    
                    // Evaluates the expression
                    evaluation = EvaluateExpression(metric_value, analyzable_experience_set.rule.operator_code, bool.Parse(analyzable_experience_set.rule.rule_value));
                }

            }
            else if (analyzable_experience_set.metric.model == "l" && analyzable_experience_set.metric.data_type == "bool")
            {
                // TODO
                // Don't know if this one exists
            }
            else if (analyzable_experience_set.metric.model == "a" && analyzable_experience_set.metric.data_type == "string")
            {
                // If Metric's is of type "Chave única", then compares the string value
                // with rule's operator to perform the expression
                string metric_value = analyzable_experience_set.metric_values.values.FirstOrDefault();

                // Evaluates the expression
                evaluation = EvaluateExpression(metric_value, analyzable_experience_set.rule.operator_code, analyzable_experience_set.rule.rule_value);
            }
            else if(analyzable_experience_set.metric.model == "l" && analyzable_experience_set.metric.data_type == "string")
            {
                // If Metric's is of type "Várias chaves", then checks if the string value
                // is contained (or not) depending on the rule's operator to perform the expression
                List<string> metric_values = analyzable_experience_set.metric_values.values;

                // Evaluates the expression
                evaluation = EvaluateExpression(metric_values, analyzable_experience_set.rule.operator_code, analyzable_experience_set.rule.rule_value);
            }

            if (evaluation == true)
            {
                // If the evaluation is true, then returns Achievment's affected Metric Value
                return analyzable_experience_set.achievment.affected_metric_value;
            }

            // If the evaluation is false, then returns null
            return null;
        }
        #endregion

        #region Calculate Panel Set by User
        private PanelSet CalculatePanelSetByUser(UserResponse user)
        {
            // Instantiates MetricValues
            MetricValues reference_metric_values = new MetricValues(
                "no_metric_values_uid",
                this.reference_metric.uid,
                user.uid,
                new List<string>(),
                DateTime.UtcNow);

            // Instantiates the inital value of reference Metric Values
            // depending on reference Metric data type
            if (this.reference_metric.data_type == "float")
            {
                reference_metric_values.values.Add("0");
            }
            else if (this.reference_metric.data_type == "string")
            {
                reference_metric_values.values.Add("");
            }

            // Clones Analyzable Verification Experiences Sets list
            // for this user to be able to analyze and delete items from this
            // list by User, without interfiring on other users
            List<AnalyzableExperienceSet> user_analyzable_experiences_sets_list = new List<AnalyzableExperienceSet>(this.analyzable_verification_experiences_sets_list);

            // Calculates User's Metric Values for reference Metric
            // using each Analyzable Experience Set
            bool hasAffectedMetric = true;
            // Does loop while there's a affected Metric on the last loop
            // because after the last loop, another Metric can now be affected
            while (hasAffectedMetric)
            {
                hasAffectedMetric = false;
                for (int i = user_analyzable_experiences_sets_list.Count - 1; i > -1; i--)
                {
                    AnalyzableExperienceSet analyzable_experience_set = user_analyzable_experiences_sets_list[i];
                    // For each User and Analyzable Experience Set,
                    // searches for its Metric Values
                    ApiResponse metricValuesApiResponse = this.metric_values_controller.GetAll(analyzable_experience_set.metric_uid, user.uid);
                    if (metricValuesApiResponse.StatusCode == 200)
                    {
                        MetricValues metric_values = ((List<MetricValues>)metricValuesApiResponse.Result).FirstOrDefault();
                        analyzable_experience_set.metric_values = metric_values;

                        // Then, checks if logics is activated
                        string achievment_effect_value = AnalyzeExperienceSetByUser(
                            analyzable_experience_set,
                            user
                        );

                        if (achievment_effect_value != null)
                        {
                            // Shows to the loop that there's an affected Metric
                            hasAffectedMetric = true;

                            // Affects the Metric Values from the affected Metric from
                            // the Achievment in the Experience Set
                            // with the amount of the Achievment effect value
                            if (analyzable_experience_set.achievment.affected_metric_uid == this.reference_metric.uid)
                            {
                                reference_metric_values = AffectMetricValues(reference_metric_values,
                                    this.reference_metric,
                                    achievment_effect_value);
                            }
                            // ELSE TODO: Could be another Metric of "c" (Calculada) classification
                            // Further calculus and modelation would be required
                            // Probably will do later!

                            // Removes Experience Set from this User's list of verification,
                            // so it's not analyzed again, generating a loop
                            user_analyzable_experiences_sets_list.RemoveAt(i);
                        }

                    } else
                    {
                        // If there's an error on metricValuesApiResponse, throws error
                        throw new System.ArgumentException("There was an error when trying to get Metric Values data", "original");
                    }
                }
            }

            // Creates Panel Set with data and returns it
            return new PanelSet(reference_metric_values, user);
        }
        #endregion

        #region Calculate Panel Sets
        private List<PanelSet> CalculatePanelSets()
        {
            List<PanelSet> panel_set_list = new List<PanelSet>();

            foreach (UserResponse user in this.users_list)
            {
                // For each User in Users list, calculate its Panel Set
                panel_set_list.Add(CalculatePanelSetByUser(user));
            }

            // Sort list by Metric Values' value descending if is "rank" type Experience
            if(this.main_experience.type == "rank")
            {
                panel_set_list.Sort((x, y) => 
                -1*float.Parse(x.metric_values.values[0], CultureInfo.InvariantCulture)
                .CompareTo(float.Parse(y.metric_values.values[0], CultureInfo.InvariantCulture)));
            }
            // Sort list by Metric Values' value descending position on main Experience
            // level ordering array if is "level" type Experience
            else if (this.main_experience.type == "level")
            {
                panel_set_list.Sort((x, y) =>
                -1 * main_experience.level_ordering
                            .FindIndex(a => a == x.metric_values.values[0])
                .CompareTo(main_experience.level_ordering.FindIndex(b => b == y.metric_values.values[0])));
            }
            return panel_set_list;
        }
        #endregion

        #region Calculate Experience Panel
        public ExperiencePanel CalculateExperiencePanel()
        {
            // Sets fields and calculates Panel Sets
            ExperiencePanel experience_panel = new ExperiencePanel();
            experience_panel.panel_sets = this.CalculatePanelSets();
            experience_panel.experience = this.main_experience;
            experience_panel.reference_metric = this.reference_metric;

            return experience_panel;
        }
        #endregion

        #region Calculate Metric Values By Complex Metric
        // Calculates all Metric Values for all Users for a specific complex Metric
        private List<MetricValues> CalculateMetricValuesByComplexMetric(Metric complex_metric)
        {
            // Instantiates Metric Values list, the method's response
            List<MetricValues> complex_metric_values_list = new List<MetricValues>();

            // Gets all necessary data to calculate it with the Calculator itself
            // then at the end, discards the unecessary data

            // First, gets All Achievments where affected Metric uid is
            // the same as complex Metric's uid
            ApiResponse achievmentApiResponse = this.achievment_controller.GetAll(complex_metric.uid);

            if (achievmentApiResponse.StatusCode == 200)
            {
                // Maps achievments uid list
                List<string> achievments_uid_list = ((List<Achievment>)achievmentApiResponse.Result).Select(h =>
                {
                    return h.uid;
                }).ToList();

                // If has found Achievments, searchs for Experiences Sets 
                // where Experience Set's achievment_uid is in Achievment's list searched before
                ApiResponse experienceSetApiResponse = this.experience_set_controller.GetByAchievmentsUidList(achievments_uid_list);

                if (experienceSetApiResponse.StatusCode == 200)
                {
                    // If has found verification Experiences Sets,
                    // creates verification Experiences Sets list
                    List<ExperienceSet> verification_experiences_sets_list = (List<ExperienceSet>)experienceSetApiResponse.Result;

                    // Then, instantiates the Calculator helper with every data found
                    Calculator complex_metric_helper = new Calculator(
                        new Experience(),
                        complex_metric,
                        verification_experiences_sets_list,
                        this.users_list,
                        this.metric_controller,
                        this.metric_values_controller,
                        this.rule_controller,
                        this.achievment_controller,
                        this.experience_set_controller);

                    // Calculates reference Metric's Metric Values for each User,
                    // and returns Experience Panel
                    ExperiencePanel experiencePanel = complex_metric_helper.CalculateExperiencePanel();

                    // Then, gets only the important data (Metric Values List)
                    complex_metric_values_list = experiencePanel.panel_sets.Select(h =>
                    {
                        return h.metric_values;
                    }).ToList();
                }
                else
                {
                    // If there's an error on any ExperienceSetApiResponse, throws error
                    throw new System.ArgumentException("There was an error when trying to get Experiences Sets data", "original");
                }
            }

            // Returns complex Metric Values list
            return complex_metric_values_list;
        }
        #endregion
        #endregion

        #region Constructor
        public Calculator(Experience main_experience, Metric reference_metric, List<ExperienceSet> verification_experiences_sets_list, List<UserResponse> users_list, MetricController metricController, MetricValuesController metricValuesController, RuleController ruleController, AchievmentController achievmentController, ExperienceSetController experienceSetController)
        {
            // Sets main Experience
            this.main_experience = main_experience;
            // Sets reference Metric
            this.reference_metric = reference_metric;
            // Sets Controllers
            this.metric_controller = metricController;
            this.rule_controller = ruleController;
            this.achievment_controller = achievmentController;
            this.metric_values_controller = metricValuesController;
            this.experience_set_controller = experienceSetController;
            // Sets Users list
            this.users_list = users_list;
            // Instantiates complex Metric's uid list
            List<string> complex_metrics_uid_list = new List<string>();
            // Instantiates complex Metrics Values list
            this.complex_metrics_values_list = new List<MetricValues>();
            // Calculates verification Experiences Sets list
            List<AnalyzableExperienceSet> analyzable_verification_experiences_sets_list = new List<AnalyzableExperienceSet>();
            foreach(ExperienceSet experience_set in verification_experiences_sets_list)
            {
                ApiResponse metricApiResponse = this.metric_controller.GetByUid(experience_set.metric_uid);
                ApiResponse ruleApiResponse = this.rule_controller.GetByUid(experience_set.rule_uid);
                ApiResponse achievmentApiResponse = this.achievment_controller.GetByUid(experience_set.achievment_uid);

                if (metricApiResponse.StatusCode == 200 && ruleApiResponse.StatusCode == 200 && achievmentApiResponse.StatusCode == 200)
                {
                    // If all Gets are successfull, then creates Analyzable Experience Set
                    // and adds it to the list
                    Metric experience_set_complex_metric = (Metric)metricApiResponse.Result;
                    analyzable_verification_experiences_sets_list.Add(
                        new AnalyzableExperienceSet(
                            experience_set,
                            experience_set_complex_metric,
                            (Rule)ruleApiResponse.Result,
                            (Achievment)achievmentApiResponse.Result
                        )
                    );

                    if (experience_set_complex_metric.classification == "c" && !complex_metrics_uid_list.Contains(experience_set_complex_metric.uid))
                    {
                        // If Metric is "Calculada" (complex)
                        // and it has not been calculated, then it needs to be calculated
                        // before the reference Metric is calculated
                        List<MetricValues> complex_metric_values_list = CalculateMetricValuesByComplexMetric(experience_set_complex_metric);
                        // Then, adds this calculated list
                        // to the whole complex Metrics Values list
                        this.complex_metrics_values_list.AddRange(complex_metric_values_list);

                        // Adds the Metric's uid to the list so it's not calculated again
                        complex_metrics_uid_list.Add(experience_set_complex_metric.uid);
                    }
                }
                else
                {
                    // If there's an error on any ApiResponse, throws error
                    throw new System.ArgumentException("There was an error when trying to get Metric, Rule or Achievment data", "original");
                }
            }
            this.analyzable_verification_experiences_sets_list = analyzable_verification_experiences_sets_list;
        }
        #endregion
    }
}
