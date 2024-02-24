export type Auction = {
  id: string,
  reservePrice: number,
  seller: string,
  winner?: string,
  soldAmount: number,
  currentHighBid: number,
  createdAt: string,
  updatedAt: string,
  auctionEnd: string,
  status: string,
  make: string,
  model: string,
  year: number
  color: string,
  mileage: number
  imageUrl: string
};

export type Bid = {
  id: string,
  auctionId: string,
  bidder: string,
  bidTime: string,
  amount: number,
  bidStatus: string
};

export type AuctionUpdate = {
  make: string,
  model: string,
  year: number
  color: string,
  mileage: number
};

export type AuctionFinished = {
  auctionId: string;
  itemSold: boolean,
  winner?: string,
  seller: string,
  soldAmount?: number
};

// export type AuctionCreate = {
//   make: string,
//   model: string,
//   year: number
//   color: string,
//   mileage: number,
//   reservePrice: number,
//   imageUrl: string,
//   auctionEnd: string,
// };

export type PagedResult<T> = {
  results: T[],
  totalCount: number,
  pageCount: number
};

export type FetchError = {
  error: {
    status: number,
    message: string
  }
}

export function isFetchError(arg: any): arg is FetchError {
  return (
    ('status' in arg) && (typeof arg.status === 'number') &&
    ('message' in arg) && (typeof arg.message === 'string')
  );
}