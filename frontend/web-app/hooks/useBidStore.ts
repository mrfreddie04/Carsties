import { Bid } from "@/types";
import { create } from "zustand";

type State = {
  bids: Bid[];
  open: boolean;
};

type Actions = {
  setBids: (bids: Bid[]) => void;
  addBid: (bid: Bid) => void;
  setOpen: (value: boolean) => void;
};

const initialState: State = {
  bids: [],
  open: true
};

export const useBidStore = create<State & Actions>()( (set) => ({
  ...initialState,
  setOpen: (value: boolean) => set(() => ({open:value})),
  setBids: (bids: Bid[]) => set(() => ({bids})),
  addBid: (bid: Bid) => 
    set((state) => {
      // console.log("SIGNALR",bid);
      // console.log("Add?",!state.bids.find(b => b.id === bid.id))
      return {
      //...state,
      bids: state.bids.find(b => b.id === bid.id) ? [...state.bids] : [bid,...state.bids]};
    })
  }));