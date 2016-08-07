// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Data;
using Avalonia.Markup.Data.Plugins;
using Xunit;

namespace Avalonia.Markup.UnitTests.Data.Plugins
{
    public class ExceptionValidationPluginTests
    {
        [Fact]
        public void Produces_BindingNotifications()
        {
            var inpcAccessorPlugin = new InpcPropertyAccessorPlugin();
            var validatorPlugin = new ExceptionValidationPlugin();
            var data = new Data();
            var accessor = inpcAccessorPlugin.Start(new WeakReference(data), nameof(data.MustBePositive));
            var validator = validatorPlugin.Start(new WeakReference(data), nameof(data.MustBePositive), accessor);
            var result = new List<object>();

            validator.Subscribe(x => result.Add(x));
            validator.SetValue(5, BindingPriority.LocalValue);
            validator.SetValue(-2, BindingPriority.LocalValue);
            validator.SetValue(6, BindingPriority.LocalValue);

            Assert.Equal(new[]
            {
                new BindingNotification(0),
                new BindingNotification(5),
                new BindingNotification(new ArgumentOutOfRangeException("value"), BindingErrorType.DataValidationError),
                new BindingNotification(6),
            }, result);
        }

        public class Data : INotifyPropertyChanged
        {
            private int _mustBePositive;

            public int MustBePositive
            {
                get { return _mustBePositive; }
                set
                {
                    if (value <= 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(value));
                    }

                    if (value != _mustBePositive)
                    {
                        _mustBePositive = value;
                        NotifyPropertyChanged();
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
