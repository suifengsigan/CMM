using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EdmDraw
{
    public class DraftingHelper
    {
        static NXOpen.UF.UFSession _ufSession = NXOpen.UF.UFSession.GetUFSession();
        public static void SetTabularColumnWidth(int col, double width, NXOpen.Tag tag)
        {
            _ufSession.Draw.WriteTabnotColWdt(tag, col + 1, width);
        }


        public static void SetTabularRowHeight(int row, double width, NXOpen.Tag tag)
        {
            _ufSession.Draw.WriteTabnotRowHgt(tag, row + 1, width);
        }

        public static void WriteTabularCell(int row, int column, string cellText, NXOpen.Tag tag,double text_Height=3.5)
        {
            row += 1;
            column += 1;
            var cellParams = new NXOpen.UF.UFDraw.TabnotCellParams();
            var eval_data = new NXOpen.UF.UFDraw.TabnotCellEvalData();
            _ufSession.Draw.ReadTabnotCell(tag, row, column, out cellParams, out eval_data);
            cellParams.cell_text = cellText;
            cellParams.horiz_just = NXOpen.UF.UFDraw.TabnotJust.TabnotJustCenter;// UF_DRAW_tabnot_just_center;
            cellParams.vert_just = NXOpen.UF.UFDraw.TabnotJust.TabnotJustMiddle; //UF_DRAW_tabnot_just_middle;
            cellParams.ug_text_height = text_Height;
            cellParams.ug_font = "helios";
            _ufSession.Draw.WriteTabnotCell(tag, row, column, ref cellParams);
        }

        public static void UpdateTabularNote(NXOpen.Tag tag)
        {
            int numberColumns, numberRows;
            _ufSession.Tabnot.AskNmColumns(tag, out numberColumns);
            _ufSession.Tabnot.AskNmRows(tag, out numberRows);
            for (int i = 0; i < numberColumns; i++)
            {
                NXOpen.Tag columnTag;
                _ufSession.Tabnot.AskNthColumn(tag, i, out columnTag);
                for (int j = 0; j < numberRows; j++)
                {
                    NXOpen.Tag rowTag;
                    _ufSession.Tabnot.AskNthRow(tag, j, out rowTag);
                    NXOpen.Tag cellTag;
                    _ufSession.Tabnot.AskCellAtRowCol(rowTag, columnTag, out cellTag);
                    var cellPrefs = new NXOpen.UF.UFTabnot.CellPrefs();
                    _ufSession.Tabnot.AskCellPrefs(cellTag, out cellPrefs);
                    cellPrefs.zero_display = NXOpen.UF.UFTabnot.ZeroDisplay.ZeroDisplayZero;// UF_TABNOT_zero_display_zero;
                    cellPrefs.text_density = 3;
                    _ufSession.Tabnot.SetCellPrefs(cellTag, ref cellPrefs);
                }
            }
            _ufSession.Draw.UpdateTabnot(tag);
            _ufSession.Tabnot.Update(tag);
        }
    }
}
