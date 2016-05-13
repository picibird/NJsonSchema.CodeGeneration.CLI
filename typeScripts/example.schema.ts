export interface Example {
    address: address;
    phoneNumber: Anonymous[];
}

export interface address {
    streetAddress: string;
    city: string;
}

export interface Anonymous {
    location: string;
    code: number;
}