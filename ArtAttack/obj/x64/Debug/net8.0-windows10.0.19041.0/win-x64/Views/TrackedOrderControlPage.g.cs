﻿#pragma checksum "C:\Users\alexe\Source\Repos\UBB-SE-2025-CtrlAltDefeat\ArtAttack\Views\TrackedOrderControlPage.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "7CAF5EEB1D27DBD60635477E91925FCE9A0DD31A86528E5ACAFA4A8BD34C4066"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ArtAttack.Views
{
    partial class TrackedOrderControlPage : 
        global::Microsoft.UI.Xaml.Controls.Page, 
        global::Microsoft.UI.Xaml.Markup.IComponentConnector
    {

        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler"," 3.0.0.2503")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 2: // Views\TrackedOrderControlPage.xaml line 46
                {
                    global::Microsoft.UI.Xaml.Controls.Button element2 = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)element2).Click += this.RevertLastCheckpointButton_Clicked;
                }
                break;
            case 3: // Views\TrackedOrderControlPage.xaml line 95
                {
                    global::Microsoft.UI.Xaml.Controls.Button element3 = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)element3).Click += this.UpdateCurrentCheckpointButton_Clicked;
                }
                break;
            case 4: // Views\TrackedOrderControlPage.xaml line 98
                {
                    this.UpdateDetails = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.StackPanel>(target);
                }
                break;
            case 5: // Views\TrackedOrderControlPage.xaml line 100
                {
                    this.TimestampRadioButtons = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.StackPanel>(target);
                }
                break;
            case 6: // Views\TrackedOrderControlPage.xaml line 111
                {
                    this.DateTimePickers = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.StackPanel>(target);
                }
                break;
            case 7: // Views\TrackedOrderControlPage.xaml line 121
                {
                    this.LocationTextBoxUpdate = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.TextBox>(target);
                }
                break;
            case 8: // Views\TrackedOrderControlPage.xaml line 122
                {
                    this.DescriptionTextBoxUpdate = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.TextBox>(target);
                }
                break;
            case 9: // Views\TrackedOrderControlPage.xaml line 125
                {
                    this.StatusComboBoxUpdate = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.ComboBox>(target);
                }
                break;
            case 10: // Views\TrackedOrderControlPage.xaml line 135
                {
                    this.confirmUpdateCurrentCheckpointButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.confirmUpdateCurrentCheckpointButton).Click += this.ConfirmUpdateCurrentCheckpointButton_Clicked;
                }
                break;
            case 11: // Views\TrackedOrderControlPage.xaml line 113
                {
                    this.TimestampDatePicker = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.CalendarDatePicker>(target);
                }
                break;
            case 12: // Views\TrackedOrderControlPage.xaml line 117
                {
                    this.TimestampTimePicker = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.TimePicker>(target);
                }
                break;
            case 13: // Views\TrackedOrderControlPage.xaml line 102
                {
                    this.ManualTimestampRadio = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.RadioButton>(target);
                    ((global::Microsoft.UI.Xaml.Controls.RadioButton)this.ManualTimestampRadio).Checked += this.ManualTimestampRadio_Checked;
                }
                break;
            case 14: // Views\TrackedOrderControlPage.xaml line 106
                {
                    this.AutoTimestampRadio = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.RadioButton>(target);
                    ((global::Microsoft.UI.Xaml.Controls.RadioButton)this.AutoTimestampRadio).Checked += this.AutoTimestampRadio_Checked;
                }
                break;
            case 15: // Views\TrackedOrderControlPage.xaml line 66
                {
                    global::Microsoft.UI.Xaml.Controls.Button element15 = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)element15).Click += this.AddNewCheckpointButton_Clicked;
                }
                break;
            case 16: // Views\TrackedOrderControlPage.xaml line 69
                {
                    this.AddDetails = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.StackPanel>(target);
                }
                break;
            case 17: // Views\TrackedOrderControlPage.xaml line 71
                {
                    this.LocationTextBoxAdd = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.TextBox>(target);
                }
                break;
            case 18: // Views\TrackedOrderControlPage.xaml line 74
                {
                    this.DescriptionTextBoxAdd = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.TextBox>(target);
                }
                break;
            case 19: // Views\TrackedOrderControlPage.xaml line 77
                {
                    this.StatusComboBoxAdd = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.ComboBox>(target);
                }
                break;
            case 20: // Views\TrackedOrderControlPage.xaml line 87
                {
                    this.confirmAddNewCheckpointButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.confirmAddNewCheckpointButton).Click += this.ConfirmAddNewCheckpointButton_Clicked;
                }
                break;
            case 21: // Views\TrackedOrderControlPage.xaml line 51
                {
                    global::Microsoft.UI.Xaml.Controls.Button element21 = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)element21).Click += this.ChangeEstimatedDeliveryDateButton_Clicked;
                }
                break;
            case 22: // Views\TrackedOrderControlPage.xaml line 55
                {
                    this.deliveryCalendarDatePicker = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.CalendarDatePicker>(target);
                }
                break;
            case 23: // Views\TrackedOrderControlPage.xaml line 59
                {
                    this.confirmChangeEstimatedDeliveryDateButton = global::WinRT.CastExtensions.As<global::Microsoft.UI.Xaml.Controls.Button>(target);
                    ((global::Microsoft.UI.Xaml.Controls.Button)this.confirmChangeEstimatedDeliveryDateButton).Click += this.ConfirmChangeEstimatedDeliveryDateButton_Clicked;
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }


        /// <summary>
        /// GetBindingConnector(int connectionId, object target)
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler"," 3.0.0.2503")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Microsoft.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Microsoft.UI.Xaml.Markup.IComponentConnector returnValue = null;
            return returnValue;
        }
    }
}

