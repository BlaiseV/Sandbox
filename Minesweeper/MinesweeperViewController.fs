﻿namespace Minesweeper

open System
open System.Drawing

open MonoTouch.UIKit
open MonoTouch.Foundation
open utils

[<Register ("MinesweeperViewController")>]
type MinesweeperViewController () =
    inherit UIViewController ()

    let mutable actionMode = ActionMode.Digging

    override this.ViewDidLoad () =
        let rec playGame() =
            let mines, neighbors = setMinesAndGetNeighbors

            let getButton i j = 
                let MinesweeperButtonClicked = 
                    new EventHandler(fun sender eventargs -> 
                        let ms = sender :?> MinesweeperButton
                        if (actionMode = ActionMode.Flagging) then
                            if (ms.CurrentImage = UIImage.FromBundle("Flag.png")) then
                                ms.SetImage(null, UIControlState.Normal)
                            else
                                ms.SetImage(UIImage.FromBundle("Flag.png"), UIControlState.Normal)
                        elif (actionMode = ActionMode.Digging && ms.IsMine) then
                            ms.BackgroundColor <- UIColor.Red
                            let x = new UIAlertView(":(", "YOU LOSE!", null, "Okay", "Cancel")
                            x.Show()
                            playGame()
                        else
                            ms.SetImage(null, UIControlState.Normal)
                            ms.BackgroundColor <- UIColor.DarkGray
                            if (ms.SurroundingMines = 0) then
                                ms.SetTitle("", UIControlState.Normal)
                            else 
                                ms.SetTitle(ms.SurroundingMines.ToString(), UIControlState.Normal)
                        )
                
                let b = new MinesweeperButton(mines.[i,j], neighbors.[i,j])
                b.BackgroundColor <- UIColor.LightGray
                b.Frame <- new RectangleF((float32)i*35.f+25.f, (float32)j*35.f+25.f, (float32)32.f, (float32)32.f)
                b.TouchUpInside.AddHandler MinesweeperButtonClicked
                b

            let CreateButtonView (view:UIView) i j = 
                let b = (getButton i j)
                view.Add b
                b.BringSubviewToFront

            let CreateSliderView (view:UIView) = 
                let s = new UISegmentedControl(new RectangleF((float32)50.f, (float32)Height*35.f+50.f, (float32)200.f, (float32)50.f))

                let HandleSegmentChanged = 
                    new EventHandler(fun sender eventargs -> 
                        let s = sender :?> UISegmentedControl
                        actionMode <- match s.SelectedSegment with 
                                        | 0 -> ActionMode.Flagging
                                        | 1 -> ActionMode.Digging
                                        | _ -> ActionMode.Flagging
                        )

                s.InsertSegment(UIImage.FromBundle("Flag.png"), 0, false)
                s.InsertSegment(UIImage.FromBundle("Bomb.png"), 1, false)
                s.SelectedSegment <- 1
                actionMode <- ActionMode.Digging
                s.ValueChanged.AddHandler HandleSegmentChanged
                view.Add s
            
            if (this.View.Subviews.Length > 0) then
                this.View.Subviews.[0].RemoveFromSuperview()
            let v = new UIView(new RectangleF(0.f, 0.f, this.View.Bounds.Width, this.View.Bounds.Height))
            let boardTiles = Array2D.init Width Height (CreateButtonView v)
            CreateSliderView v
            this.View.AddSubview v

        playGame()
        base.ViewDidLoad ()

    override this.ShouldAutorotateToInterfaceOrientation (orientation) =
        orientation <> UIInterfaceOrientation.PortraitUpsideDown
    