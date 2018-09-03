﻿//==============================================================================
//  WARNING!!  This file is overwritten by the Block UI Styler while generating
//  the automation code. Any modifications to this file will be lost after
//  generating the code again.
//
//       Filename:  F:\PHEactProject\Source\Project\UGUI\CMMProgramUI.cs
//
//        This file was generated by the NX Block UI Styler
//        Created by: PENGHUI
//              Version: NX 9
//              Date: 01-02-2018  (Format: mm-dd-yyyy)
//              Time: 14:53 (Format: hh-mm)
//
//==============================================================================

//==============================================================================
//  Purpose:  This TEMPLATE file contains C# source to guide you in the
//  construction of your Block application dialog. The generation of your
//  dialog file (.dlx extension) is the first step towards dialog construction
//  within NX.  You must now create a NX Open application that
//  utilizes this file (.dlx).
//
//  The information in this file provides you with the following:
//
//  1.  Help on how to load and display your Block UI Styler dialog in NX
//      using APIs provided in NXOpen.BlockStyler namespace
//  2.  The empty callback methods (stubs) associated with your dialog items
//      have also been placed in this file. These empty methods have been
//      created simply to start you along with your coding requirements.
//      The method name, argument list and possible return values have already
//      been provided for you.
//==============================================================================

//------------------------------------------------------------------------------
//These imports are needed for the following template code
//------------------------------------------------------------------------------
using System;
using NXOpen;
using NXOpen.BlockStyler;

//------------------------------------------------------------------------------
//Represents Block Styler application class
//------------------------------------------------------------------------------
public partial class CMMProgramUI
{
    //class members
    private static Session theSession = null;
    private static UI theUI = null;
    private string theDlxFileName;
    private NXOpen.BlockStyler.BlockDialog theDialog;
    private NXOpen.BlockStyler.Group group0;// Block type: Group
    private NXOpen.BlockStyler.SelectObject selectCuprum;// Block type: Selection
    private NXOpen.BlockStyler.Enumeration enumSelectTool;// Block type: Enumeration
    private NXOpen.BlockStyler.Group group;// Block type: Group
    private NXOpen.BlockStyler.DrawingArea drawingArea0;// Block type: Drawing Area
    private NXOpen.BlockStyler.Toggle toggle0;// Block type: Toggle
    private NXOpen.BlockStyler.Button btnAutoSelectPoint;// Block type: Button
    private NXOpen.BlockStyler.SelectObject selectionPoint;// Block type: Selection
    private NXOpen.BlockStyler.Tree tree_control0;// Block type: Tree Control
    private NXOpen.BlockStyler.Button btnUP;// Block type: Button
    private NXOpen.BlockStyler.Button btnDown;// Block type: Button
    private NXOpen.BlockStyler.Button btnRemove;// Block type: Button
    private NXOpen.BlockStyler.Button btnExport;// Block type: Button
    //------------------------------------------------------------------------------
    //Bit Option for Property: SnapPointTypesEnabled
    //------------------------------------------------------------------------------
    public static readonly int SnapPointTypesEnabled_UserDefined = (1 << 0);
    public static readonly int SnapPointTypesEnabled_Inferred = (1 << 1);
    public static readonly int SnapPointTypesEnabled_ScreenPosition = (1 << 2);
    public static readonly int SnapPointTypesEnabled_EndPoint = (1 << 3);
    public static readonly int SnapPointTypesEnabled_MidPoint = (1 << 4);
    public static readonly int SnapPointTypesEnabled_ControlPoint = (1 << 5);
    public static readonly int SnapPointTypesEnabled_Intersection = (1 << 6);
    public static readonly int SnapPointTypesEnabled_ArcCenter = (1 << 7);
    public static readonly int SnapPointTypesEnabled_QuadrantPoint = (1 << 8);
    public static readonly int SnapPointTypesEnabled_ExistingPoint = (1 << 9);
    public static readonly int SnapPointTypesEnabled_PointonCurve = (1 << 10);
    public static readonly int SnapPointTypesEnabled_PointonSurface = (1 << 11);
    public static readonly int SnapPointTypesEnabled_PointConstructor = (1 << 12);
    public static readonly int SnapPointTypesEnabled_TwocurveIntersection = (1 << 13);
    public static readonly int SnapPointTypesEnabled_TangentPoint = (1 << 14);
    public static readonly int SnapPointTypesEnabled_Poles = (1 << 15);
    public static readonly int SnapPointTypesEnabled_BoundedGridPoint = (1 << 16);
    public static readonly int SnapPointTypesEnabled_FacetVertexPoint = (1 << 17);
    //------------------------------------------------------------------------------
    //Bit Option for Property: SnapPointTypesOnByDefault
    //------------------------------------------------------------------------------
    public static readonly int SnapPointTypesOnByDefault_EndPoint = (1 << 3);
    public static readonly int SnapPointTypesOnByDefault_MidPoint = (1 << 4);
    public static readonly int SnapPointTypesOnByDefault_ControlPoint = (1 << 5);
    public static readonly int SnapPointTypesOnByDefault_Intersection = (1 << 6);
    public static readonly int SnapPointTypesOnByDefault_ArcCenter = (1 << 7);
    public static readonly int SnapPointTypesOnByDefault_QuadrantPoint = (1 << 8);
    public static readonly int SnapPointTypesOnByDefault_ExistingPoint = (1 << 9);
    public static readonly int SnapPointTypesOnByDefault_PointonCurve = (1 << 10);
    public static readonly int SnapPointTypesOnByDefault_PointonSurface = (1 << 11);
    public static readonly int SnapPointTypesOnByDefault_PointConstructor = (1 << 12);
    public static readonly int SnapPointTypesOnByDefault_BoundedGridPoint = (1 << 16);

    //------------------------------------------------------------------------------
    //Constructor for NX Styler class
    //------------------------------------------------------------------------------
    public CMMProgramUI()
    {
        try
        {
            theSession = Session.GetSession();
            theUI = UI.GetUI();
            theDlxFileName = "CMMProgramUI.dlx";
            InitEvent(theDlxFileName, initialize_cb, (s) =>
            {
                return theDialog = theUI.CreateDialog(s);
            });
        }
        catch (Exception ex)
        {
            //---- Enter your exception handling code here -----
            throw ex;
        }
    }
    //------------------------------- DIALOG LAUNCHING ---------------------------------
    //
    //    Before invoking this application one needs to open any part/empty part in NX
    //    because of the behavior of the blocks.
    //
    //    Make sure the dlx file is in one of the following locations:
    //        1.) From where NX session is launched
    //        2.) $UGII_USER_DIR/application
    //        3.) For released applications, using UGII_CUSTOM_DIRECTORY_FILE is highly
    //            recommended. This variable is set to a full directory path to a file 
    //            containing a list of root directories for all custom applications.
    //            e.g., UGII_CUSTOM_DIRECTORY_FILE=$UGII_ROOT_DIR\menus\custom_dirs.dat
    //
    //    You can create the dialog using one of the following way:
    //
    //    1. Journal Replay
    //
    //        1) Replay this file through Tool->Journal->Play Menu.
    //
    //    2. USER EXIT
    //
    //        1) Create the Shared Library -- Refer "Block UI Styler programmer's guide"
    //        2) Invoke the Shared Library through File->Execute->NX Open menu.
    //
    //------------------------------------------------------------------------------
    public static void Main()
    {
        CMMProgramUI theCMMProgramUI = null;
        try
        {
            theCMMProgramUI = new CMMProgramUI();
            // The following method shows the dialog immediately
            theCMMProgramUI.Show();
        }
        catch (Exception ex)
        {
            //---- Enter your exception handling code here -----
            theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
        }
        finally
        {
            if (theCMMProgramUI != null)
                theCMMProgramUI.Dispose();
            theCMMProgramUI = null;
        }
    }
    //------------------------------------------------------------------------------
    // This method specifies how a shared image is unloaded from memory
    // within NX. This method gives you the capability to unload an
    // internal NX Open application or user  exit from NX. Specify any
    // one of the three constants as a return value to determine the type
    // of unload to perform:
    //
    //
    //    Immediately : unload the library as soon as the automation program has completed
    //    Explicitly  : unload the library from the "Unload Shared Image" dialog
    //    AtTermination : unload the library when the NX session terminates
    //
    //
    // NOTE:  A program which associates NX Open applications with the menubar
    // MUST NOT use this option since it will UNLOAD your NX Open application image
    // from the menubar.
    //------------------------------------------------------------------------------
    public static int GetUnloadOption(string arg)
    {
        //return System.Convert.ToInt32(Session.LibraryUnloadOption.Explicitly);
        return System.Convert.ToInt32(Session.LibraryUnloadOption.Immediately);
        // return System.Convert.ToInt32(Session.LibraryUnloadOption.AtTermination);
    }

    //------------------------------------------------------------------------------
    // Following method cleanup any housekeeping chores that may be needed.
    // This method is automatically called by NX.
    //------------------------------------------------------------------------------
    public static void UnloadLibrary(string arg)
    {
        try
        {
            //---- Enter your code here -----
        }
        catch (Exception ex)
        {
            //---- Enter your exception handling code here -----
            theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
        }
    }

    //------------------------------------------------------------------------------
    //This method shows the dialog on the screen
    //------------------------------------------------------------------------------
    public NXOpen.UIStyler.DialogResponse Show()
    {
        try
        {
            theDialog.Show();
        }
        catch (Exception ex)
        {
            //---- Enter your exception handling code here -----
            theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
        }
        return 0;
    }

    //------------------------------------------------------------------------------
    //Method Name: Dispose
    //------------------------------------------------------------------------------
    public void Dispose()
    {
        if (theDialog != null)
        {
            theDialog.Dispose();
            theDialog = null;
        }
    }

    //------------------------------------------------------------------------------
    //---------------------Block UI Styler Callback Functions--------------------------
    //------------------------------------------------------------------------------

    //------------------------------------------------------------------------------
    //Callback Name: initialize_cb
    //------------------------------------------------------------------------------
    public void initialize_cb()
    {
        try
        {
            group0 = (NXOpen.BlockStyler.Group)theDialog.TopBlock.FindBlock("group0");
            selectCuprum = (NXOpen.BlockStyler.SelectObject)theDialog.TopBlock.FindBlock("selectCuprum");
            enumSelectTool = (NXOpen.BlockStyler.Enumeration)theDialog.TopBlock.FindBlock("enumSelectTool");
            group = (NXOpen.BlockStyler.Group)theDialog.TopBlock.FindBlock("group");
            drawingArea0 = (NXOpen.BlockStyler.DrawingArea)theDialog.TopBlock.FindBlock("drawingArea0");
            toggle0 = (NXOpen.BlockStyler.Toggle)theDialog.TopBlock.FindBlock("toggle0");
            btnAutoSelectPoint = (NXOpen.BlockStyler.Button)theDialog.TopBlock.FindBlock("btnAutoSelectPoint");
            selectionPoint = (NXOpen.BlockStyler.SelectObject)theDialog.TopBlock.FindBlock("selectionPoint");
            tree_control0 = (NXOpen.BlockStyler.Tree)theDialog.TopBlock.FindBlock("tree_control0");
            btnUP = (NXOpen.BlockStyler.Button)theDialog.TopBlock.FindBlock("btnUP");
            btnDown = (NXOpen.BlockStyler.Button)theDialog.TopBlock.FindBlock("btnDown");
            btnRemove = (NXOpen.BlockStyler.Button)theDialog.TopBlock.FindBlock("btnRemove");
            btnExport = (NXOpen.BlockStyler.Button)theDialog.TopBlock.FindBlock("btnExport");
            //------------------------------------------------------------------------------
            //Registration of Treelist specific callbacks
            //------------------------------------------------------------------------------
            //tree_control0.SetOnExpandHandler(new NXOpen.BlockStyler.Tree.OnExpandCallback(OnExpandCallback));

            //tree_control0.SetOnInsertColumnHandler(new NXOpen.BlockStyler.Tree.OnInsertColumnCallback(OnInsertColumnCallback));

            //tree_control0.SetOnInsertNodeHandler(new NXOpen.BlockStyler.Tree.OnInsertNodeCallback(OnInsertNodecallback));

            //tree_control0.SetOnDeleteNodeHandler(new NXOpen.BlockStyler.Tree.OnDeleteNodeCallback(OnDeleteNodecallback));

            //tree_control0.SetOnPreSelectHandler(new NXOpen.BlockStyler.Tree.OnPreSelectCallback(OnPreSelectcallback));

            //tree_control0.SetOnSelectHandler(new NXOpen.BlockStyler.Tree.OnSelectCallback(OnSelectcallback));

            //tree_control0.SetOnStateChangeHandler(new NXOpen.BlockStyler.Tree.OnStateChangeCallback(OnStateChangecallback));

            //tree_control0.SetToolTipTextHandler(new NXOpen.BlockStyler.Tree.ToolTipTextCallback(ToolTipTextcallback));

            //tree_control0.SetColumnSortHandler(new NXOpen.BlockStyler.Tree.ColumnSortCallback(ColumnSortcallback));

            //tree_control0.SetStateIconNameHandler(new NXOpen.BlockStyler.Tree.StateIconNameCallback(StateIconNameCallback));

            //tree_control0.SetOnBeginLabelEditHandler(new NXOpen.BlockStyler.Tree.OnBeginLabelEditCallback(OnBeginLabelEditCallback));

            //tree_control0.SetOnEndLabelEditHandler(new NXOpen.BlockStyler.Tree.OnEndLabelEditCallback(OnEndLabelEditCallback));

            //tree_control0.SetOnEditOptionSelectedHandler(new NXOpen.BlockStyler.Tree.OnEditOptionSelectedCallback(OnEditOptionSelectedCallback));

            //tree_control0.SetAskEditControlHandler(new NXOpen.BlockStyler.Tree.AskEditControlCallback(AskEditControlCallback));

            //tree_control0.SetOnMenuHandler(new NXOpen.BlockStyler.Tree.OnMenuCallback(OnMenuCallback));;

            //tree_control0.SetOnMenuSelectionHandler(new NXOpen.BlockStyler.Tree.OnMenuSelectionCallback(OnMenuSelectionCallback));;

            //tree_control0.SetIsDropAllowedHandler(new NXOpen.BlockStyler.Tree.IsDropAllowedCallback(IsDropAllowedCallback));;

            //tree_control0.SetIsDragAllowedHandler(new NXOpen.BlockStyler.Tree.IsDragAllowedCallback(IsDragAllowedCallback));;

            //tree_control0.SetOnDropHandler(new NXOpen.BlockStyler.Tree.OnDropCallback(OnDropCallback));;

            //tree_control0.SetOnDropMenuHandler(new NXOpen.BlockStyler.Tree.OnDropMenuCallback(OnDropMenuCallback));

            //tree_control0.SetOnDefaultActionHandler(new NXOpen.BlockStyler.Tree.OnDefaultActionCallback(OnDefaultActionCallback));

            //------------------------------------------------------------------------------
        }
        catch (Exception ex)
        {
            //---- Enter your exception handling code here -----
            theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
        }
    }

    //------------------------------------------------------------------------------
    //Callback Name: dialogShown_cb
    //This callback is executed just before the dialog launch. Thus any value set 
    //here will take precedence and dialog will be launched showing that value. 
    //------------------------------------------------------------------------------
    public void dialogShown_cb()
    {
        try
        {
            //---- Enter your callback code here -----
        }
        catch (Exception ex)
        {
            //---- Enter your exception handling code here -----
            theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
        }
    }

    //------------------------------------------------------------------------------
    //Callback Name: update_cb
    //------------------------------------------------------------------------------
    public int update_cb(NXOpen.BlockStyler.UIBlock block)
    {
        try
        {
            if (block == selectCuprum)
            {
                //---------Enter your code here-----------
            }
            else if (block == enumSelectTool)
            {
                //---------Enter your code here-----------
            }
            else if (block == drawingArea0)
            {
                //---------Enter your code here-----------
            }
            else if (block == toggle0)
            {
                //---------Enter your code here-----------
            }
            else if (block == btnAutoSelectPoint)
            {
                //---------Enter your code here-----------
            }
            else if (block == selectionPoint)
            {
                //---------Enter your code here-----------
            }
            else if (block == btnUP)
            {
                //---------Enter your code here-----------
            }
            else if (block == btnDown)
            {
                //---------Enter your code here-----------
            }
            else if (block == btnRemove)
            {
                //---------Enter your code here-----------
            }
            else if (block == btnExport)
            {
                //---------Enter your code here-----------
            }
        }
        catch (Exception ex)
        {
            //---- Enter your exception handling code here -----
            theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
        }
        return 0;
    }
    //------------------------------------------------------------------------------
    //Treelist specific callbacks
    //------------------------------------------------------------------------------
    //public void OnExpandCallback(NXOpen.BlockStyler.Tree tree, NXOpen.BlockStyler.Node node)
    //{
    //}

    //public void OnInsertColumnCallback(NXOpen.BlockStyler.Tree tree, NXOpen.BlockStyler.Node node, int columnID)
    //{
    //}

    //public void OnInsertNodecallback(NXOpen.BlockStyler.Tree tree, NXOpen.BlockStyler.Node node)
    //{
    //}

    //public void OnDeleteNodecallback(NXOpen.BlockStyler.Tree tree, NXOpen.BlockStyler.Node node)
    //{
    //}

    //public void OnPreSelectcallback(NXOpen.BlockStyler.Tree tree, NXOpen.BlockStyler.Node node, int columnID, bool Selected)
    //{
    //}

    //public void OnSelectcallback(NXOpen.BlockStyler.Tree tree, NXOpen.BlockStyler.Node node, int columnID, bool Selected)
    //{
    //}

    //public void OnStateChangecallback(NXOpen.BlockStyler.Tree tree, NXOpen.BlockStyler.Node node, int State)
    //{
    //}

    //public string ToolTipTextcallback(NXOpen.BlockStyler.Tree tree, NXOpen.BlockStyler.Node node, int columnID)
    //{
    //}

    //public int ColumnSortcallback(NXOpen.BlockStyler.Tree tree, int columnID, NXOpen.BlockStyler.Node node1, NXOpen.BlockStyler.Node node2)
    //{
    //}

    //public string StateIconNameCallback(NXOpen.BlockStyler.Tree tree, NXOpen.BlockStyler.Node node, int state)
    //{
    //}

    //public Tree.BeginLabelEditState OnBeginLabelEditCallback(NXOpen.BlockStyler.Tree tree, NXOpen.BlockStyler.Node node, int columnID)
    //{
    //}

    //public Tree.EndLabelEditState OnEndLabelEditCallback(NXOpen.BlockStyler.Tree tree, NXOpen.BlockStyler.Node node, int columnID, string editedText)
    //{
    //}

    //public Tree.EditControlOption OnEditOptionSelectedCallback(NXOpen.BlockStyler.Tree tree, NXOpen.BlockStyler.Node node, int columnID, int selectedOptionID, string selectedOptionText, Tree.ControlType type)
    //{
    //}

    //public Tree.ControlType AskEditControlCallback(NXOpen.BlockStyler.Tree tree, NXOpen.BlockStyler.Node node, int columnID)
    //{
    //}

    //public void OnMenuCallback(NXOpen.BlockStyler.Tree tree, NXOpen.BlockStyler.Node node, int columnID)
    //{
    //}

    //public void OnMenuSelectionCallback(NXOpen.BlockStyler.Tree tree, NXOpen.BlockStyler.Node node, int menuItemID)
    //{
    //}

    //public Node.DropType IsDropAllowedCallback(NXOpen.BlockStyler.Tree tree, NXOpen.BlockStyler.Node node, int columnID, NXOpen.BlockStyler.Node targetNode, int targetColumnID)
    //{
    //}

    //public Node.DragType IsDragAllowedCallback(NXOpen.BlockStyler.Tree tree, NXOpen.BlockStyler.Node node, int columnID)
    //{
    //}

    //public bool OnDropCallback(NXOpen.BlockStyler.Tree tree, NXOpen.BlockStyler.Node[] node, int columnID, NXOpen.BlockStyler.Node targetNode, int targetColumnID, Node.DropType dropType, int dropMenuItemId)
    //{
    //}

    //public void OnDropMenuCallback(NXOpen.BlockStyler.Tree tree, NXOpen.BlockStyler.Node node, int columnID, NXOpen.BlockStyler.Node targetNode, int targetColumnID)
    //{
    //}

    //public void OnDefaultActionCallback(NXOpen.BlockStyler.Tree tree, NXOpen.BlockStyler.Node node, int columnID)
    //{
    //}


    //------------------------------------------------------------------------------
    //Function Name: GetBlockProperties
    //Returns the propertylist of the specified BlockID
    //------------------------------------------------------------------------------
    public PropertyList GetBlockProperties(string blockID)
    {
        PropertyList plist = null;
        try
        {
            plist = theDialog.GetBlockProperties(blockID);
        }
        catch (Exception ex)
        {
            //---- Enter your exception handling code here -----
            theUI.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
        }
        return plist;
    }

}
