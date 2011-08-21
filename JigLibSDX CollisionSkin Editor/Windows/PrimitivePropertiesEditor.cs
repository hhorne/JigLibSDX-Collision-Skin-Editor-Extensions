using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using JigLibSDX.Geometry;

namespace JigLibSDX_CSE
{
    public partial class PrimitivePropertiesEditor : Form
    {
        private PrimitiveProperties _primitiveProperties;
        private bool _init;
        private bool _comma;

        public PrimitiveProperties PrimitiveProperties
        {
            get { return _primitiveProperties; }
        }

        public PrimitivePropertiesEditor(PrimitiveProperties primitiveProperties)
        {
            _init = true;

            InitializeComponent();

            _primitiveProperties = primitiveProperties;

            DictionaryEntry[] tempDict = new DictionaryEntry[] 
                { new DictionaryEntry((int)PrimitiveProperties.MassTypeEnum.Mass, PrimitiveProperties.MassTypeEnum.Mass.ToString()), 
                  new DictionaryEntry((int)PrimitiveProperties.MassTypeEnum.Density, PrimitiveProperties.MassTypeEnum.Density.ToString()) };

            massTypeCombo.DataSource = tempDict;
            massTypeCombo.DisplayMember = "Value";
            massTypeCombo.ValueMember = "Key";
            massTypeCombo.SelectedIndex = 0;

            if (_primitiveProperties.MassType == PrimitiveProperties.MassTypeEnum.Mass)
            {
                massTypeCombo.SelectedIndex = 0;
                valueLabel.Text = "Mass:";
            }
            else
            {
                massTypeCombo.SelectedIndex = 1;
                valueLabel.Text = "Density:";
            }


            tempDict = new DictionaryEntry[] 
                { new DictionaryEntry(PrimitiveProperties.MassDistributionEnum.Solid, PrimitiveProperties.MassDistributionEnum.Solid.ToString()), 
                  new DictionaryEntry(PrimitiveProperties.MassDistributionEnum.Shell, PrimitiveProperties.MassDistributionEnum.Shell.ToString()) };

            massDistributionCombo.DataSource = tempDict;
            massDistributionCombo.DisplayMember = "Value";
            massDistributionCombo.ValueMember = "Key";

            if (_primitiveProperties.MassDistribution == PrimitiveProperties.MassDistributionEnum.Solid)
            {
                massDistributionCombo.SelectedIndex = 0;
            }
            else
            {
                massDistributionCombo.SelectedIndex = 1;
            }

            valueBox.Text = _primitiveProperties.MassOrDensity.ToString();

            if (Convert.ToSingle("1,5") == 1.5f)
            {
                separationHelp.Text = "Decimal place separator: Comma.";
                _comma = true;
            }
            else
            {
                separationHelp.Text = "Decimal place separator: Dot.";
                _comma = false;
            }

            _init = false;
        }

        private void massTypeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_init)
            {
                _primitiveProperties.MassType = (PrimitiveProperties.MassTypeEnum)massTypeCombo.SelectedValue;

                if (_primitiveProperties.MassType == PrimitiveProperties.MassTypeEnum.Mass)
                {
                    valueLabel.Text = "Mass:";
                }
                else
                {
                    valueLabel.Text = "Density:";
                }
            }
        }

        private void massDistributionCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_init)
            {
                _primitiveProperties.MassDistribution = (PrimitiveProperties.MassDistributionEnum)massDistributionCombo.SelectedValue;
            }
        }

        private void valueBox_TextChanged(object sender, EventArgs e)
        {
            if (!_init)
            {
                try
                {
                    if (valueBox.Text.Length > 0)
                    {
                        _primitiveProperties.MassOrDensity = Convert.ToSingle(valueBox.Text);
                    }
                    else
                    { 
                        _primitiveProperties.MassOrDensity = 0f;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Float values only.", "Invalid Value", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        private void valueBox_Leave(object sender, EventArgs e)
        {
            int textLength = valueBox.Text.Length;
            int valueLength = _primitiveProperties.MassOrDensity.ToString().Length;

            if (valueBox.Text != "" && textLength != valueLength)
            {
                MessageBox.Show(this, "The value you just put in had to be adjusted.\r\n" + 
                    "Maybe you used the wrong decimal place separator.\r\n" + 
                    "\r\n" + 
                    "Input: " + valueBox.Text + "\r\n" +
                    "Interpretation: " + _primitiveProperties.MassOrDensity.ToString(), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            valueBox.Text = _primitiveProperties.MassOrDensity.ToString();
        }
    }
}
