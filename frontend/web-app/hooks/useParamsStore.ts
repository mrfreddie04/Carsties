import { create } from "zustand";

type State = {
  pageNumber: number;
  pageSize: number;
  pageCount: number;
  orderBy: string;
  filterBy: string;
  searchTerm: string;
  searchValue: string;
  seller?: string;
  winner?: string;
};

type Actions = {
  setParams: (params: Partial<State>) => void;
  setSearchValue: (searchValue: string) => void;
  reset: () => void;
};

const initialState: State = {
  pageNumber: 1,
  pageSize: 12,
  pageCount: 1,
  orderBy: "endingSoon",
  filterBy: "live",
  searchTerm: "",
  searchValue: "",
  seller: undefined,
  winner: undefined
}

export const useParamsStore = create<State & Actions>()( (set) => ({
  ...initialState,
  setParams: (newParams: Partial<State>) => {
    set((state) => { 
      if(newParams.pageNumber) {
        return {...state, pageNumber: newParams.pageNumber};
      } else {
        return {...state, ...newParams, pageNumber: 1};
      }  
    })
  },
  //setSearchValue: (searchValue: string) => set(state => ({...state, searchValue})),
  setSearchValue: (searchValue: string) => set({searchValue}),
  reset: () => set(initialState)
}));