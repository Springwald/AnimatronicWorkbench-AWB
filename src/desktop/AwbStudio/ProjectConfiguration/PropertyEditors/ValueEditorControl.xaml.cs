using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AwbStudio.ProjectConfiguration.PropertyEditors
{
    /// <summary>
    /// Interaction logic for ValueEditorControl.xaml
    /// </summary>
    public partial class ValueEditorControl : UserControl
    {
        public string PropertyName
        {
            get { return (string)GetValue(PropertyNameProperty); }
            set { SetValue(PropertyNameProperty, value); }
        }

        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.Register("PropertyName", typeof(string), typeof(ValueEditorControl), new PropertyMetadata(null));

        public string PropertyContent
        {
            get { return (string)GetValue(PropertyContentProperty); }
            set { SetValue(PropertyContentProperty, value); }
        }

        public static readonly DependencyProperty PropertyContentProperty =
            DependencyProperty.Register("PropertyContent", typeof(string), typeof(ValueEditorControl), new PropertyMetadata(null));

        public ValueEditorControl()
        {
            InitializeComponent();

        }

        public void SetPropertyToEdit<T>(Expression<Func<T>> property)
        {
            var propertyInfo = ((MemberExpression)property.Body).Member as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("The lambda expression 'property' should point to a valid Property");

            PropertyName = propertyInfo.Name;

            switch (property.Parameters.Count())
            {
                case 0: PropertyContent = string.Empty;
                    break;
                case 1: PropertyContent = property.Parameters[0].ToString();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(property.Parameters.Count() + " parameters not supported yet.");
            }

            if (property.Parameters.Count == 0)
                PropertyContent =  string.Empty;
            else if (property.Parameters.Count == 1)
                PropertyContent = propertyInfo?.GetValue(property.Parameters[0])?.ToString() ?? string.Empty;
        }
    }
}
