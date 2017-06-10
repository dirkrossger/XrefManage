using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xReference
{
    class cFile
    {
        public static List<string> GetFiles()
        {
            List<string> _Filenames = null;

            System.Windows.Forms.OpenFileDialog _dialog;
            _dialog = new System.Windows.Forms.OpenFileDialog();
            _dialog.Multiselect = true;
            _dialog.InitialDirectory = "C:/";
            _dialog.Title = "Select Autocad dwg ritdef file";
            _dialog.Filter = "dwg file | *.dwg";
            _dialog.FilterIndex = 2;
            _dialog.RestoreDirectory = true;

            if (_Filenames == null)
                _Filenames = new List<string>();

            if (_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    for (int i = 0; i < _dialog.FileNames.Count(); i++)
                    {
                        String file = _dialog.FileNames[i];
                        _Filenames.Add(_dialog.FileNames[i]);
                    }

                    var result = _Filenames.GroupBy(x => x).Where(group => group.Count() > 1).Select(group => group.Key);
                    _Filenames = _Filenames.Distinct().ToList();
                }
                catch (System.Exception ex)
                { }
            }
            return _Filenames;
        }
    }
}
