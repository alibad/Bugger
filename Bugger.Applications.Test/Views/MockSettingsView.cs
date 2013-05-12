﻿using BigEgg.Framework.UnitTesting.Views;
using Bugger.Applications.Views;
using System.ComponentModel.Composition;

namespace Bugger.Applications.Test.Views
{
    [Export(typeof(ISettingsView)), Export]
    public class MockSettingsView : MockView, ISettingsView
    {
    }
}
